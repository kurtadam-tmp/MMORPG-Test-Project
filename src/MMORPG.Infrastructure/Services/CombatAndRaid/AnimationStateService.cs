using System.Collections.Concurrent;
using MMORPG.Domain.Interfaces;

namespace MMORPG.Infrastructure.Services;

public class AnimationStateService : IAnimationStateService
{
    private readonly ConcurrentDictionary<Guid, AnimationState> _states = new();

    public AnimationState StartAttackAnimation(Guid characterId, string skillOrAttackId, float windupMs, float activeMs, float recoveryMs)
    {
        var state = new AnimationState
        {
            CharacterId = characterId,
            CurrentAnimation = skillOrAttackId,
            Phase = AnimationPhase.Windup
        };

        _states[characterId] = state;
        Console.WriteLine($"[AnimationEngine] Started attack animation '{skillOrAttackId}' for Character '{characterId}' (Windup: {windupMs}ms, Active: {activeMs}ms, Recovery: {recoveryMs}ms).");
        return state;
    }

    public bool TryCancelAnimation(Guid characterId, string inputType, out string cancelReason)
    {
        if (_states.TryGetValue(characterId, out var state))
        {
            // 1. WINDUP CANCEL (Feint / Fake Cast / Emergency Dodge)
            if (state.Phase == AnimationPhase.Windup && (inputType == "Move" || inputType == "Dodge" || inputType == "Block"))
            {
                _states.TryRemove(characterId, out _);
                cancelReason = $"[Windup Cancel / Feint] Attack cast cancelled during Windup phase via '{inputType}'! No damage dealt, weapon reset.";
                Console.WriteLine($"[AnimationEngine FEINT] Character '{characterId}' CANCELLED attack during WINDUP phase via '{inputType}'!");
                return true;
            }

            // 2. RECOVERY CANCEL (Post-Damage Animation Cancel for Max DPS)
            if (state.Phase == AnimationPhase.Recovery && (inputType == "Move" || inputType == "Dodge" || inputType == "Skill"))
            {
                _states.TryRemove(characterId, out _);
                cancelReason = $"[Recovery Cancel] Post-damage swing cancelled via '{inputType}' input for instant combo chain!";
                Console.WriteLine($"[AnimationEngine RECOVERY CANCEL] Character '{characterId}' CANCELLED attack during RECOVERY phase via '{inputType}'!");
                return true;
            }
        }

        cancelReason = "Cannot cancel animation during Active damage frame.";
        return false;
    }

    public bool CancelInteraction(Guid characterId, string cancelReason)
    {
        if (_states.TryGetValue(characterId, out var state) && state.IsChannelingInteraction)
        {
            state.IsChannelingInteraction = false;
            Console.WriteLine($"[InteractionEngine INTERRUPTED] Character '{characterId}' channeling interrupted: {cancelReason}!");
            return true;
        }

        return false;
    }

    public AnimationState? GetAnimationState(Guid characterId)
    {
        _states.TryGetValue(characterId, out var state);
        return state;
    }
}
