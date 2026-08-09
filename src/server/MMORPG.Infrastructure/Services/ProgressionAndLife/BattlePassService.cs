using System.Collections.Concurrent;
using MMORPG.Domain.Interfaces;

namespace MMORPG.Infrastructure.Services;

public class BattlePassService : IBattlePassService
{
    private readonly ConcurrentDictionary<Guid, PlayerBattlePassState> _states = new();

    public PlayerBattlePassState ProcessDailyLogin(Guid characterId, out string rewardMessage)
    {
        rewardMessage = string.Empty;
        var state = _states.GetOrAdd(characterId, id => new PlayerBattlePassState { CharacterId = id });

        lock (state)
        {
            var now = DateTime.UtcNow;
            if ((now - state.LastLoginDate).TotalHours >= 24)
            {
                if ((now - state.LastLoginDate).TotalHours <= 48)
                {
                    state.LoginStreakDays++;
                }
                else
                {
                    state.LoginStreakDays = 1; // Streak reset if missed a day
                }
                state.LastLoginDate = now;
            }

            rewardMessage = GetDailyRewardForDay(state.LoginStreakDays);
            Console.WriteLine($"[BattlePass DAILY LOGIN] Character '{characterId}' logged in on Day {state.LoginStreakDays}! Reward: {rewardMessage}.");
            return state;
        }
    }

    public bool AddBattlePassXp(Guid characterId, long xpGained, out string tierUpMessage)
    {
        tierUpMessage = string.Empty;
        var state = _states.GetOrAdd(characterId, id => new PlayerBattlePassState { CharacterId = id });

        lock (state)
        {
            state.BattlePassXp += xpGained;
            long requiredXpForNextTier = state.BattlePassTier * 1000;

            if (state.BattlePassXp >= requiredXpForNextTier && state.BattlePassTier < 100)
            {
                state.BattlePassTier++;
                state.BattlePassXp -= requiredXpForNextTier;
                tierUpMessage = $"TEBRİKLER! Savaş Bileti Seviye {state.BattlePassTier}'e yükseldiniz!";
                Console.WriteLine($"[BattlePass TIER UP] Character '{characterId}' advanced to Battle Pass Tier {state.BattlePassTier}!");
                return true;
            }
        }
        return false;
    }

    public bool ClaimTierReward(Guid characterId, int tier, bool isPremium, out string rewardName)
    {
        rewardName = string.Empty;
        if (_states.TryGetValue(characterId, out var state))
        {
            lock (state)
            {
                if (tier > state.BattlePassTier)
                {
                    rewardName = "Bu Seviyeye henüz ulaşılmadı.";
                    return false;
                }

                if (isPremium && !state.IsPremiumUnlocked)
                {
                    rewardName = "Premium Savaş Bileti aktif değil.";
                    return false;
                }

                var targetList = isPremium ? state.ClaimedPremiumTiers : state.ClaimedFreeTiers;
                if (targetList.Contains(tier))
                {
                    rewardName = "Bu ödül zaten talep edilmiş.";
                    return false;
                }

                targetList.Add(tier);
                rewardName = isPremium ? $"Tier {tier} Premium Ödülü (Efsanevi Görünüm)" : $"Tier {tier} Ücretsiz Ödül (1,000 Altın)";
                Console.WriteLine($"[BattlePass CLAIM] Character '{characterId}' claimed {(isPremium ? "Premium" : "Free")} Tier {tier} Reward!");
                return true;
            }
        }
        return false;
    }

    public bool UpgradeToPremiumPass(Guid characterId)
    {
        var state = _states.GetOrAdd(characterId, id => new PlayerBattlePassState { CharacterId = id });
        lock (state)
        {
            state.IsPremiumUnlocked = true;
            Console.WriteLine($"[BattlePass PREMIUM UNLOCKED] Character '{characterId}' upgraded to PREMIUM BATTLE PASS!");
            return true;
        }
    }

    private string GetDailyRewardForDay(int day)
    {
        return day switch
        {
            1 => "500 Altın",
            7 => "Kutsanmış Artı Basma Parşömeni x3",
            14 => "Zırhlı Savaş Atı Bineği",
            30 => "Efsanevi Unvan 'Eternal Conqueror' & 5,000 Altın",
            _ => $"{day * 100} Altın Ödülü"
        };
    }
}
