using System.Collections.Concurrent;
using MMORPG.Domain.Interfaces;

namespace MMORPG.Infrastructure.Services;

public class SkillCooldownService : ISkillCooldownService
{
    private readonly ConcurrentDictionary<(Guid CharacterId, int SkillId), DateTime> _cooldowns = new();

    public bool CanCastSkill(Guid characterId, int skillId, out float remainingCooldownSeconds)
    {
        remainingCooldownSeconds = 0f;
        var key = (characterId, skillId);

        if (_cooldowns.TryGetValue(key, out var readyTime))
        {
            var now = DateTime.UtcNow;
            if (now < readyTime)
            {
                remainingCooldownSeconds = (float)(readyTime - now).TotalSeconds;
                return false;
            }
        }

        return true;
    }

    public void TriggerSkillCooldown(Guid characterId, int skillId, float cooldownDurationSeconds)
    {
        var key = (characterId, skillId);
        _cooldowns[key] = DateTime.UtcNow.AddSeconds(cooldownDurationSeconds);
        Console.WriteLine($"[SkillCooldown] Skill #{skillId} triggered for Character '{characterId}' ({cooldownDurationSeconds}s Cooldown).");
    }
}
