namespace MMORPG.Domain.Interfaces;

public interface IWriteBehindService
{
    Task MarkCharacterDirtyAsync(Guid characterId);
    Task<int> FlushDirtyCharactersAsync();
}
