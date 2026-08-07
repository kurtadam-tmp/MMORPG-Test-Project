using MMORPG.Domain.Entities;

namespace MMORPG.Domain.Interfaces;

public interface IStatRepository
{
    Task<Stat?> GetByCharacterIdAsync(Guid characterId);
    Task<bool> UpdateAsync(Stat stat);
    Task<bool> UpdateGoldAsync(Guid characterId, long newGoldAmount);
    Task<bool> AllocatePointsAsync(Guid characterId, int strDiff, int agiDiff, int intDiff, int vitDiff);
}
