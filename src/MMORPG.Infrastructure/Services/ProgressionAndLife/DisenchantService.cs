using MMORPG.Domain.Interfaces;

namespace MMORPG.Infrastructure.Services;

public class DisenchantService : IDisenchantService
{
    public DisenchantResult DisenchantItem(Guid characterId, Guid itemInstanceId, string itemRarity)
    {
        var result = new DisenchantResult { Success = true };

        switch (itemRarity.ToLowerInvariant())
        {
            case "legendary":
                result.ReagentsObtained.Add(("item_dragon_scale", 1));
                result.ReagentsObtained.Add(("item_arcane_dust", 10));
                result.Message = "Disenchanted Legendary Equipment -> Received 1x Dragon Scale + 10x Arcane Dust!";
                break;
            case "epic":
                result.ReagentsObtained.Add(("item_lava_core", 1));
                result.ReagentsObtained.Add(("item_arcane_dust", 5));
                result.Message = "Disenchanted Epic Equipment -> Received 1x Lava Core + 5x Arcane Dust!";
                break;
            case "rare":
                result.ReagentsObtained.Add(("item_arcane_dust", 3));
                result.Message = "Disenchanted Rare Equipment -> Received 3x Arcane Dust!";
                break;
            default:
                result.ReagentsObtained.Add(("item_scrap_metal", 2));
                result.Message = "Disenchanted Common Equipment -> Received 2x Scrap Metal.";
                break;
        }

        Console.WriteLine($"[Disenchant SUCCESS] Character '{characterId}' disenchanted item '{itemInstanceId}' ({itemRarity}). {result.Message}");
        return result;
    }
}
