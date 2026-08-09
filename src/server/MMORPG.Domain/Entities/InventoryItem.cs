namespace MMORPG.Domain.Entities;

public class InventoryItem
{
    public Guid Id { get; set; }
    public Guid InstanceId { get; set; } = Guid.NewGuid();
    public Guid CharacterId { get; set; }
    public int SlotIndex { get; set; }
    public string ItemId { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    public int Durability { get; set; } = 100;
    public string AttributesJson { get; set; } = "{}";
    public bool IsEquipped { get; set; } = false;
    public string? EquipSlot { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
