namespace MMORPG.Domain.Interfaces;

public interface ISkillCooldownService
{
    bool CanCastSkill(Guid characterId, int skillId, out float remainingCooldownSeconds);
    void TriggerSkillCooldown(Guid characterId, int skillId, float cooldownDurationSeconds);
}
