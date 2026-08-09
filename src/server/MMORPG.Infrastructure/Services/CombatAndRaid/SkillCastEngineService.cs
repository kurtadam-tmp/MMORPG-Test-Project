using System.Collections.Concurrent;
using MMORPG.Domain.Interfaces;

namespace MMORPG.Infrastructure.Services.CombatAndRaid;

public class SkillCastEngineService : ISkillCastEngineService
{
    private readonly ConcurrentDictionary<Guid, ActiveSkillCastState> _activeCasts = new();

    public ActiveSkillCastState StartSkillCast(Guid characterId, string skillName, double baseCastTime, int hasteRating)
    {
        double actualCastTime = baseCastTime * (100.0 / (100.0 + hasteRating));
        var state = new ActiveSkillCastState
        {
            CharacterId = characterId,
            SkillName = skillName,
            BaseCastTimeSeconds = baseCastTime,
            ActualCastTimeSeconds = actualCastTime,
            CastStartTime = DateTime.UtcNow,
            ExpectedCastEndTime = DateTime.UtcNow.AddSeconds(actualCastTime),
            IsInterrupted = false,
            PushbackCount = 0
        };

        _activeCasts[characterId] = state;
        Console.WriteLine($"[SKILL CAST ENGINE] Character '{characterId}' started casting '{skillName}' (Base: {baseCastTime:F1}s, Actual: {actualCastTime:F2}s with {hasteRating} Haste).");
        return state;
    }

    public bool ApplyDamageCastPushback(Guid characterId, out double newRemainingCastTime)
    {
        newRemainingCastTime = 0;
        if (_activeCasts.TryGetValue(characterId, out var state) && !state.IsInterrupted)
        {
            lock (state)
            {
                if (state.PushbackCount < 2) // Max 2 pushbacks per cast
                {
                    state.PushbackCount++;
                    state.ExpectedCastEndTime = state.ExpectedCastEndTime.AddSeconds(0.5); // +0.5s pushback delay
                    newRemainingCastTime = (state.ExpectedCastEndTime - DateTime.UtcNow).TotalSeconds;
                    Console.WriteLine($"[CAST PUSHBACK!] Character '{characterId}' took damage while casting '{state.SkillName}'! Cast delayed +0.5s (Remaining: {newRemainingCastTime:F2}s).");
                    return true;
                }
            }
        }
        return false;
    }

    public bool InterruptCast(Guid characterId, string interruptReason)
    {
        if (_activeCasts.TryGetValue(characterId, out var state))
        {
            lock (state)
            {
                state.IsInterrupted = true;
                _activeCasts.TryRemove(characterId, out _);
                Console.WriteLine($"[CAST INTERRUPTED!] Character '{characterId}' cast of '{state.SkillName}' INTERRUPTED due to '{interruptReason}'!");
                return true;
            }
        }
        return false;
    }

    public bool CheckCastCompletion(Guid characterId, out string completedSkillName)
    {
        completedSkillName = string.Empty;
        if (_activeCasts.TryGetValue(characterId, out var state) && !state.IsInterrupted)
        {
            if (DateTime.UtcNow >= state.ExpectedCastEndTime)
            {
                completedSkillName = state.SkillName;
                _activeCasts.TryRemove(characterId, out _);
                Console.WriteLine($"[SKILL CAST COMPLETED!] Character '{characterId}' successfully cast '{completedSkillName}'!");
                return true;
            }
        }
        return false;
    }

    public double CalculateWeaponSwingTime(double baseSwingTime, int attackSpeedBonusPercent)
    {
        double actualSwingTime = baseSwingTime * (100.0 / (100.0 + attackSpeedBonusPercent));
        Console.WriteLine($"[WEAPON SWING TIME] Base: {baseSwingTime:F2}s -> Actual: {actualSwingTime:F2}s (+{attackSpeedBonusPercent}% Attack Speed).");
        return actualSwingTime;
    }
}
