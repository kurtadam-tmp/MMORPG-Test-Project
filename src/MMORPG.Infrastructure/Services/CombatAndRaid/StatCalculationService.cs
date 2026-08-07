using MMORPG.Domain.Interfaces;

namespace MMORPG.Infrastructure.Services;

public class StatCalculationService : IStatCalculationService
{
    public CalculatedCharacterStats CalculateStats(int strength, int agility, int intelligence, int vitality, int level)
    {
        int baseHp = 100 + (level * 20);
        int baseMp = 50 + (level * 10);

        return new CalculatedCharacterStats
        {
            MaxHealth = baseHp + (vitality * 25),
            MaxMana = baseMp + (intelligence * 15),
            PhysicalAttackPower = (level * 5) + (strength * 3),
            MagicAttackPower = (level * 5) + (intelligence * 4),
            Armor = vitality * 2,
            CriticalChancePercent = MathF.Min(50.0f, 5.0f + (agility * 0.5f)),
            MovementSpeed = 5.0f + MathF.Min(3.0f, agility * 0.05f)
        };
    }

    public int CalculateMitigatedDamage(int rawDamage, int targetArmor)
    {
        if (targetArmor <= 0) return rawDamage;

        // Standard MMORPG armor mitigation formula: Damage * (100 / (100 + Armor))
        float damageReductionRatio = 100.0f / (100.0f + targetArmor);
        int finalDamage = Math.Max(1, Math.Min(rawDamage, (int)(rawDamage * damageReductionRatio)));
        return finalDamage;
    }
}
