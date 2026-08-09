namespace MMORPG.Domain.Interfaces;

public class ActiveSkillCastState
{
    public Guid CharacterId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public double BaseCastTimeSeconds { get; set; }
    public double ActualCastTimeSeconds { get; set; }
    public DateTime CastStartTime { get; set; }
    public DateTime ExpectedCastEndTime { get; set; }
    public bool IsInterrupted { get; set; }
    public int PushbackCount { get; set; }
}

public interface ISkillCastEngineService
{
    ActiveSkillCastState StartSkillCast(Guid characterId, string skillName, double baseCastTime, int hasteRating);
    bool ApplyDamageCastPushback(Guid characterId, out double newRemainingCastTime);
    bool InterruptCast(Guid characterId, string interruptReason);
    bool CheckCastCompletion(Guid characterId, out string completedSkillName);
    double CalculateWeaponSwingTime(double baseSwingTime, int attackSpeedBonusPercent);
}
