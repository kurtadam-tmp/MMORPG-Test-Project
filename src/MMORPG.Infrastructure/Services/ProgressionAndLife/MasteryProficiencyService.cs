using System.Collections.Concurrent;
using MMORPG.Domain.Interfaces;

namespace MMORPG.Infrastructure.Services.ProgressionAndLife;

public class MasteryProficiencyService : IMasteryProficiencyService
{
    private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<string, WeaponMasteryProgress>> _weaponMasteries = new();
    private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<string, SkillMasteryProgress>> _skillMasteries = new();

    public WeaponMasteryProgress RecordWeaponUse(Guid characterId, string weaponCategory, out string levelUpAnnouncement)
    {
        levelUpAnnouncement = string.Empty;
        var charDict = _weaponMasteries.GetOrAdd(characterId, id => new ConcurrentDictionary<string, WeaponMasteryProgress>());
        var progress = charDict.GetOrAdd(weaponCategory, cat => new WeaponMasteryProgress { WeaponCategory = cat, MasteryLevel = 1, CurrentXp = 0 });

        lock (progress)
        {
            progress.CurrentXp += 1;
            long requiredXp = progress.MasteryLevel * 50;

            if (progress.CurrentXp >= requiredXp && progress.MasteryLevel < 100)
            {
                progress.MasteryLevel++;
                progress.CurrentXp -= requiredXp;
                progress.BonusDamagePercent = (int)(progress.MasteryLevel * 0.5); // +0.5% damage per level
                progress.BonusCritChancePercent = (int)(progress.MasteryLevel * 0.2); // +0.2% crit per level

                if (progress.MasteryLevel == 20) progress.UnlockedMasteryTitle = "Adept";
                else if (progress.MasteryLevel == 50) progress.UnlockedMasteryTitle = "Master";
                else if (progress.MasteryLevel == 100) progress.UnlockedMasteryTitle = "Grandmaster";

                levelUpAnnouncement = $"USTALIK ATLAMASI! '{weaponCategory}' silah kategorisinde Seviye {progress.MasteryLevel} [{progress.UnlockedMasteryTitle}] rütbesine ulaştınız (+{progress.BonusDamagePercent}% Hasar)!";
                Console.WriteLine($"[WEAPON MASTERY UP] Character '{characterId}' advanced '{weaponCategory}' to Level {progress.MasteryLevel}!");
            }
        }

        return progress;
    }

    public SkillMasteryProgress RecordSkillUse(Guid characterId, string skillName, out string rankUpAnnouncement)
    {
        rankUpAnnouncement = string.Empty;
        var charDict = _skillMasteries.GetOrAdd(characterId, id => new ConcurrentDictionary<string, SkillMasteryProgress>());
        var progress = charDict.GetOrAdd(skillName, name => new SkillMasteryProgress { SkillName = name, SkillRank = 1, UseCount = 0 });

        lock (progress)
        {
            progress.UseCount += 1;
            long requiredUses = progress.SkillRank * 25;

            if (progress.UseCount >= requiredUses && progress.SkillRank < 10)
            {
                progress.SkillRank++;
                progress.UseCount -= requiredUses;
                progress.DamageBonusPercent = progress.SkillRank * 5; // +5% damage per rank
                progress.ManaCostReductionPercent = progress.SkillRank * 2; // -2% mana cost per rank

                rankUpAnnouncement = $"YETENEK UZMANLIĞI! '{skillName}' yeteneğinde Rank {progress.SkillRank}'e yükseldiniz (+{progress.DamageBonusPercent}% Hasar, -{progress.ManaCostReductionPercent}% Mana)!";
                Console.WriteLine($"[SKILL MASTERY UP] Character '{characterId}' advanced '{skillName}' to Rank {progress.SkillRank}!");
            }
        }

        return progress;
    }

    public List<WeaponMasteryProgress> GetCharacterWeaponMasteries(Guid characterId)
    {
        if (_weaponMasteries.TryGetValue(characterId, out var dict))
        {
            return dict.Values.ToList();
        }
        return new List<WeaponMasteryProgress>();
    }

    public List<SkillMasteryProgress> GetCharacterSkillMasteries(Guid characterId)
    {
        if (_skillMasteries.TryGetValue(characterId, out var dict))
        {
            return dict.Values.ToList();
        }
        return new List<SkillMasteryProgress>();
    }
}
