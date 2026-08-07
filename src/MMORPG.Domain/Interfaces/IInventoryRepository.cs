using MMORPG.Domain.Entities;

namespace MMORPG.Domain.Interfaces;

public interface IInventoryRepository
{
    Task<IEnumerable<InventoryItem>> GetByCharacterIdAsync(Guid characterId);
    Task<InventoryItem?> GetSlotAsync(Guid characterId, int slotIndex);
    Task<InventoryItem?> GetByInstanceIdAsync(Guid instanceId);
    Task<bool> AddOrUpdateItemAsync(InventoryItem item);
    Task<bool> SwapSlotsAsync(Guid characterId, int fromSlot, int toSlot);
    Task<bool> ToggleEquipStatusAsync(Guid itemId, bool isEquipped, string? equipSlot);
}
