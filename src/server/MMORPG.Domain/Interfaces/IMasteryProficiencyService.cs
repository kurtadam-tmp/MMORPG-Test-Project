namespace MMORPG.Domain.Interfaces;

public class WeaponMasteryProgress
{
    public string WeaponCategory { get; set; } = string.Empty;
    public int MasteryLevel { get; set; } = 1;
    public long CurrentXp { get; set; }
    public string UnlockedMasteryTitle { get; set; } = "Novice";
    public int BonusDamagePercent { get; set; }
    public int BonusCritChancePercent { get; set; }
}

public class SkillMasteryProgress
{
    public string SkillName { get; set; } = string.Empty;
    public int SkillRank { get; set; } = 1;
    public long UseCount { get; set; }
    public int DamageBonusPercent { get; set; }
    public int ManaCostReductionPercent { get; set; }
}

public interface IMasteryProficiencyService
{
    WeaponMasteryProgress RecordWeaponUse(Guid characterId, string weaponCategory, out string levelUpAnnouncement);
    SkillMasteryProgress RecordSkillUse(Guid characterId, string skillName, out string rankUpAnnouncement);
    List<WeaponMasteryProgress> GetCharacterWeaponMasteries(Guid characterId);
    List<SkillMasteryProgress> GetCharacterSkillMasteries(Guid characterId);
}
