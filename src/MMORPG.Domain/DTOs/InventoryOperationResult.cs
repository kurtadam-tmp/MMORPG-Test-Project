using MMORPG.Domain.Entities;

namespace MMORPG.Domain.DTOs;

public class InventoryOperationResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public InventoryItem? AffectedItem { get; set; }
    public IEnumerable<InventoryItem>? InventoryItems { get; set; }
}
