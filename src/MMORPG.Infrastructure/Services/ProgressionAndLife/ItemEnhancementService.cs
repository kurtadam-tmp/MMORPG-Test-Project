using MMORPG.Domain.Interfaces;

namespace MMORPG.Infrastructure.Services;

public class ItemEnhancementService : IItemEnhancementService
{
    private static readonly Dictionary<int, float> SuccessRates = new()
    {
        { 0, 1.00f }, // +1 (100%)
        { 1, 1.00f }, // +2 (100%)
        { 2, 1.00f }, // +3 (100%)
        { 3, 0.70f }, // +4 (70%)
        { 4, 0.50f }, // +5 (50%)
        { 5, 0.35f }, // +6 (35%)
        { 6, 0.20f }, // +7 (20%)
        { 7, 0.10f }, // +8 (10%)
        { 8, 0.04f }  // +9 (4%)
    };

    public EnhancementResult EnhanceItem(Guid characterId, Guid itemInstanceId, int currentLevel, bool useProtectionScroll)
    {
        if (currentLevel >= 9)
        {
            return new EnhancementResult { Success = false, NewEnhancementLevel = currentLevel, ItemDestroyed = false, Message = "Item is already at maximum enhancement (+9)!" };
        }

        float chance = SuccessRates.TryGetValue(currentLevel, out var rate) ? rate : 0.05f;
        float roll = Random.Shared.NextSingle();

        if (roll <= chance)
        {
            int nextLvl = currentLevel + 1;
            Console.WriteLine($"[Enhancement SUCCESS] Item '{itemInstanceId}' upgraded to +{nextLvl} for Character '{characterId}'!");
            return new EnhancementResult { Success = true, NewEnhancementLevel = nextLvl, ItemDestroyed = false, Message = $"SUCCESS! Item upgraded to +{nextLvl}!" };
        }
        else
        {
            if (useProtectionScroll)
            {
                Console.WriteLine($"[Enhancement FAILED] Item '{itemInstanceId}' upgrade failed, but Blessed Protection Scroll saved the item!");
                return new EnhancementResult { Success = false, NewEnhancementLevel = currentLevel, ItemDestroyed = false, Message = "Enhancement failed! Protected by Blessed Scroll." };
            }

            if (currentLevel >= 6)
            {
                Console.WriteLine($"[Enhancement CRITICAL FAILURE] Item '{itemInstanceId}' shattered into dust!");
                return new EnhancementResult { Success = false, NewEnhancementLevel = 0, ItemDestroyed = true, Message = "CRITICAL FAILURE! Item shattered into dust!" };
            }

            int degradedLvl = Math.Max(0, currentLevel - 1);
            Console.WriteLine($"[Enhancement FAILED] Item '{itemInstanceId}' degraded to +{degradedLvl}.");
            return new EnhancementResult { Success = false, NewEnhancementLevel = degradedLvl, ItemDestroyed = false, Message = $"Enhancement failed! Item degraded to +{degradedLvl}." };
        }
    }

    public int CalculateBonusStatForLevel(int baseStat, int enhancementLevel)
    {
        // Each +1 adds +15% compound stat boost
        float multiplier = 1.0f + (enhancementLevel * 0.15f);
        return (int)(baseStat * multiplier);
    }
}
