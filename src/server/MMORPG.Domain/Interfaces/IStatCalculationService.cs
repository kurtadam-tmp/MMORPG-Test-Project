namespace MMORPG.Domain.Interfaces;

public class CalculatedCharacterStats
{
    public int MaxHealth { get; set; }
    public int MaxMana { get; set; }
    public int PhysicalAttackPower { get; set; }
    public int MagicAttackPower { get; set; }
    public int Armor { get; set; }
    public float CriticalChancePercent { get; set; }
    public float MovementSpeed { get; set; }
}

public interface IStatCalculationService
{
    CalculatedCharacterStats CalculateStats(int strength, int agility, int intelligence, int vitality, int level);
    int CalculateMitigatedDamage(int rawDamage, int targetArmor);
}
