namespace MMORPG.Domain.DTOs;

public class CombatResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int DamageDealt { get; set; }
    public bool IsCritical { get; set; }
    public bool IsDodged { get; set; }
    public int TargetCurrentHp { get; set; }
    public bool TargetIsDead { get; set; }
    public int CooldownRemainingMs { get; set; }
}
