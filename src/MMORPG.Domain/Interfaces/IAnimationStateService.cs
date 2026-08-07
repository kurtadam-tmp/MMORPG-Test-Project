namespace MMORPG.Domain.Interfaces;

public enum AnimationPhase
{
    Idle,
    Windup,     // Feint / Cancel window before active frame
    Active,     // Damage execution frame
    Recovery    // Post-swing combo cancel window
}

public class AnimationState
{
    public Guid CharacterId { get; set; }
    public string CurrentAnimation { get; set; } = string.Empty;
    public AnimationPhase Phase { get; set; } = AnimationPhase.Idle;
    public bool IsChannelingInteraction { get; set; }
}

public interface IAnimationStateService
{
    AnimationState StartAttackAnimation(Guid characterId, string skillOrAttackId, float windupMs, float activeMs, float recoveryMs);
    bool TryCancelAnimation(Guid characterId, string inputType, out string cancelReason);
    bool CancelInteraction(Guid characterId, string cancelReason);
    AnimationState? GetAnimationState(Guid characterId);
}
