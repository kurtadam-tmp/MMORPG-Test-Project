using MMORPG.Domain.Entities;

namespace MMORPG.Domain.Interfaces;

public interface ICharacterRepository
{
    Task<Character?> GetByIdAsync(Guid id);
    Task<IEnumerable<Character>> GetByPlayerIdAsync(Guid playerId);
    Task<Guid> CreateWithStatsAsync(Character character, Stat stat);
    Task<bool> UpdatePositionAsync(Guid characterId, float posX, float posY, float posZ, int zoneId);
    Task<bool> UpdateExperienceAndLevelAsync(Guid characterId, long experience, int level);
}
