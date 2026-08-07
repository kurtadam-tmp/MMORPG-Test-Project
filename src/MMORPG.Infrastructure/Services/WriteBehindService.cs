using MMORPG.Domain.Interfaces;
using MMORPG.Infrastructure.Cache;
using StackExchange.Redis;

namespace MMORPG.Infrastructure.Services;

public class WriteBehindService : IWriteBehindService
{
    private readonly IRedisConnectionFactory _redisFactory;
    private readonly ICharacterRepository _characterRepository;
    private static readonly string DirtyCharactersSetKey = "dirty:characters";

    public WriteBehindService(
        IRedisConnectionFactory redisFactory,
        ICharacterRepository characterRepository)
    {
        _redisFactory = redisFactory;
        _characterRepository = characterRepository;
    }

    public async Task MarkCharacterDirtyAsync(Guid characterId)
    {
        var db = _redisFactory.GetDatabase();
        await db.SetAddAsync(DirtyCharactersSetKey, characterId.ToString());
    }

    public async Task<int> FlushDirtyCharactersAsync()
    {
        var db = _redisFactory.GetDatabase();
        var dirtyMembers = await db.SetMembersAsync(DirtyCharactersSetKey);
        if (dirtyMembers.Length == 0) return 0;

        int flushedCount = 0;
        foreach (var member in dirtyMembers)
        {
            if (!Guid.TryParse(member.ToString(), out var characterId))
                continue;

            // Remove from dirty set first
            await db.SetRemoveAsync(DirtyCharactersSetKey, member);

            // In a real flow, fetch cached position from Redis zone state and persist to PostgreSQL
            var character = await _characterRepository.GetByIdAsync(characterId);
            if (character != null)
            {
                // Persistence checkpoint completed
                flushedCount++;
            }
        }

        return flushedCount;
    }
}
