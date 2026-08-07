using MMORPG.Domain.DTOs;

namespace MMORPG.Domain.Interfaces;

public interface ICombatEngineService
{
    Task<CombatResult> ExecuteSkillCastAsync(CastSkillRequest request);
    Task<bool> IsSkillOnCooldownAsync(Guid characterId, string skillId);
}
