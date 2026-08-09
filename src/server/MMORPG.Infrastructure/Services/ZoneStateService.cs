using MMORPG.Domain.Interfaces;
using MMORPG.Infrastructure.Cache;
using StackExchange.Redis;

namespace MMORPG.Infrastructure.Services;

public class ZoneStateService : IZoneStateService
{
    private readonly IRedisConnectionFactory _connectionFactory;

    public ZoneStateService(IRedisConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    private static string GetZoneSetKey(int zoneId) => $"zone:{zoneId}:players";
    private static string GetPosKey(int zoneId, Guid characterId) => $"zone:{zoneId}:pos:{characterId}";

    public async Task<bool> RegisterPlayerInZoneAsync(int zoneId, Guid characterId, float posX, float posY, float posZ)
    {
        var db = _connectionFactory.GetDatabase();
        var setKey = GetZoneSetKey(zoneId);
        var posKey = GetPosKey(zoneId, characterId);

        var added = await db.SetAddAsync(setKey, characterId.ToString());
        var posValue = $"{posX}:{posY}:{posZ}";
        await db.StringSetAsync(posKey, posValue, TimeSpan.FromHours(12));

        return added;
    }

    public async Task<bool> RemovePlayerFromZoneAsync(int zoneId, Guid characterId)
    {
        var db = _connectionFactory.GetDatabase();
        var setKey = GetZoneSetKey(zoneId);
        var posKey = GetPosKey(zoneId, characterId);

        var removed = await db.SetRemoveAsync(setKey, characterId.ToString());
        await db.KeyDeleteAsync(posKey);

        return removed;
    }

    public async Task<long> GetZonePlayerCountAsync(int zoneId)
    {
        var db = _connectionFactory.GetDatabase();
        return await db.SetLengthAsync(GetZoneSetKey(zoneId));
    }

    public async Task<IEnumerable<Guid>> GetPlayersInZoneAsync(int zoneId)
    {
        var db = _connectionFactory.GetDatabase();
        var members = await db.SetMembersAsync(GetZoneSetKey(zoneId));
        
        var result = new List<Guid>();
        foreach (var member in members)
        {
            if (Guid.TryParse(member.ToString(), out var characterId))
            {
                result.Add(characterId);
            }
        }
        return result;
    }
}
