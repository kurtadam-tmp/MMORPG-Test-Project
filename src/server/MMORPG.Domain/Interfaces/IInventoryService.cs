using MMORPG.Domain.DTOs;

namespace MMORPG.Domain.Interfaces;

public interface IInventoryService
{
    Task<InventoryOperationResult> GetCharacterInventoryAsync(string sessionToken, Guid characterId);
    Task<InventoryOperationResult> SwapSlotsAsync(SwapSlotsRequest request);
    Task<InventoryOperationResult> EquipItemAsync(EquipItemRequest request);
    Task<InventoryOperationResult> UnequipItemAsync(string sessionToken, Guid characterId, Guid itemInstanceId, int targetSlot);
    Task<InventoryOperationResult> ApplyDurabilityLossAsync(Guid itemInstanceId, int durabilityAmount);
}
