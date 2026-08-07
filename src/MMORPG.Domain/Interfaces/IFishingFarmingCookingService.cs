namespace MMORPG.Domain.Interfaces;

public class FarmPlotNode
{
    public Guid PlotId { get; set; } = Guid.NewGuid();
    public Guid OwnerCharacterId { get; set; }
    public string PlantedCropName { get; set; } = string.Empty;
    public DateTime PlantedTime { get; set; }
    public bool IsHarvestable { get; set; }
}

public class CookedDish
{
    public string DishName { get; set; } = string.Empty;
    public string BuffDescription { get; set; } = string.Empty;
    public int StatBonus { get; set; }
    public int DurationMinutes { get; set; } = 30;
}

public interface IFishingFarmingCookingService
{
    bool CatchFish(Guid characterId, string rodType, string baitType, out string caughtFishName);
    FarmPlotNode PlantCrop(Guid characterId, string cropName);
    bool HarvestCrop(Guid plotId, out string harvestedCropName);
    bool CookDish(Guid characterId, string recipeName, out CookedDish dish);
}
