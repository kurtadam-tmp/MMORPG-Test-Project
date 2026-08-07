namespace MMORPG.Domain.Interfaces;

public class LootDropResult
{
    public string ItemTemplateId { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string Rarity { get; set; } = "Common";
    public int Quantity { get; set; } = 1;
    public long GoldReward { get; set; }
}

public interface ILootEngineService
{
    IEnumerable<LootDropResult> GenerateLootForMob(string mobTypeId, int mobLevel);
}
