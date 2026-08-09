namespace MMORPG.Domain.Interfaces;

public class PlayerBattlePassState
{
    public Guid CharacterId { get; set; }
    public int LoginStreakDays { get; set; } = 1;
    public DateTime LastLoginDate { get; set; } = DateTime.UtcNow;
    public int BattlePassTier { get; set; } = 1;
    public long BattlePassXp { get; set; }
    public bool IsPremiumUnlocked { get; set; }
    public List<int> ClaimedFreeTiers { get; set; } = new();
    public List<int> ClaimedPremiumTiers { get; set; } = new();
}

public interface IBattlePassService
{
    PlayerBattlePassState ProcessDailyLogin(Guid characterId, out string rewardMessage);
    bool AddBattlePassXp(Guid characterId, long xpGained, out string tierUpMessage);
    bool ClaimTierReward(Guid characterId, int tier, bool isPremium, out string rewardName);
    bool UpgradeToPremiumPass(Guid characterId);
}
