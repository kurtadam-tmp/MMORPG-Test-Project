using MMORPG.Domain.Models;

namespace MMORPG.Domain.Interfaces;

public interface IMobEngineService
{
    void InitializeZoneMobs(int zoneId, int mobCount);
    Task ProcessZoneMobAiTickAsync(int zoneId, float deltaTime);
    IEnumerable<MobEntity> GetActiveZoneMobs(int zoneId);
    Task<bool> ApplyDamageToMobAsync(Guid mobInstanceId, int damage, Guid attackerCharacterId);
}
