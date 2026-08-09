using System.Collections.Concurrent;
using MMORPG.Domain.Interfaces;

namespace MMORPG.Infrastructure.Services;

public class FishingFarmingCookingService : IFishingFarmingCookingService
{
    private readonly ConcurrentDictionary<Guid, FarmPlotNode> _farmPlots = new();
    private static readonly Random _rng = new();

    private static readonly Dictionary<string, CookedDish> RecipeBook = new()
    {
        {
            "Grilled Golden Trout", new CookedDish
            {
                DishName = "Grilled Golden Trout",
                BuffDescription = "+15% Movement Speed & +500 Max HP",
                StatBonus = 500,
                DurationMinutes = 30
            }
        },
        {
            "Dragon Chili Stew", new CookedDish
            {
                DishName = "Dragon Chili Stew",
                BuffDescription = "+25% Fire & Physical Damage",
                StatBonus = 25,
                DurationMinutes = 30
            }
        },
        {
            "Abyssal Anglerfish Feast", new CookedDish
            {
                DishName = "Abyssal Anglerfish Feast",
                BuffDescription = "+50% Mana Regen & +30 Spell Power",
                StatBonus = 30,
                DurationMinutes = 30
            }
        }
    };

    public bool CatchFish(Guid characterId, string rodType, string baitType, out string caughtFishName)
    {
        string[] pool = { "Salmon", "Golden Trout", "Abyssal Anglerfish", "Kraken Tentacle" };
        int index = _rng.Next(0, pool.Length);
        caughtFishName = pool[index];

        Console.WriteLine($"[FISHING SUCCESS] Character '{characterId}' caught '{caughtFishName}' using '{rodType}' with '{baitType}'!");
        return true;
    }

    public FarmPlotNode PlantCrop(Guid characterId, string cropName)
    {
        var plot = new FarmPlotNode
        {
            PlotId = Guid.NewGuid(),
            OwnerCharacterId = characterId,
            PlantedCropName = cropName,
            PlantedTime = DateTime.UtcNow,
            IsHarvestable = false
        };

        _farmPlots[plot.PlotId] = plot;
        Console.WriteLine($"[FARMING PLANT] Character '{characterId}' planted '{cropName}' on plot '{plot.PlotId}'.");
        return plot;
    }

    public bool HarvestCrop(Guid plotId, out string harvestedCropName)
    {
        harvestedCropName = string.Empty;
        if (_farmPlots.TryGetValue(plotId, out var plot))
        {
            lock (plot)
            {
                plot.IsHarvestable = true;
                harvestedCropName = plot.PlantedCropName;
                _farmPlots.TryRemove(plotId, out _);
                Console.WriteLine($"[FARMING HARVEST] Harvested '{harvestedCropName}' from plot '{plotId}'!");
                return true;
            }
        }
        return false;
    }

    public bool CookDish(Guid characterId, string recipeName, out CookedDish dish)
    {
        dish = null!;
        if (RecipeBook.TryGetValue(recipeName, out var template))
        {
            dish = template;
            Console.WriteLine($"[COOKING SUCCESS] Character '{characterId}' cooked '{dish.DishName}' [{dish.BuffDescription}] (Duration: {dish.DurationMinutes}m)!");
            return true;
        }
        return false;
    }
}
