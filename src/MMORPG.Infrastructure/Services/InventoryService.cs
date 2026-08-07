using MMORPG.Domain.DTOs;
using MMORPG.Domain.Entities;
using MMORPG.Domain.Interfaces;

namespace MMORPG.Infrastructure.Services;

public class InventoryService : IInventoryService
{
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IPlayerSessionService _sessionService;
    private const int MaxInventorySlots = 100;

    public InventoryService(
        IInventoryRepository inventoryRepository,
        IPlayerSessionService sessionService)
    {
        _inventoryRepository = inventoryRepository;
        _sessionService = sessionService;
    }

    public async Task<InventoryOperationResult> GetCharacterInventoryAsync(string sessionToken, Guid characterId)
    {
        var session = await _sessionService.GetSessionAsync(sessionToken);
        if (session == null || session.ActiveCharacterId != characterId)
        {
            return new InventoryOperationResult { Success = false, Message = "Unauthorized session." };
        }

        var items = await _inventoryRepository.GetByCharacterIdAsync(characterId);
        return new InventoryOperationResult
        {
            Success = true,
            Message = "Inventory retrieved successfully.",
            InventoryItems = items
        };
    }

    public async Task<InventoryOperationResult> SwapSlotsAsync(SwapSlotsRequest request)
    {
        var session = await _sessionService.GetSessionAsync(request.SessionToken);
        if (session == null || session.ActiveCharacterId != request.CharacterId)
        {
            return new InventoryOperationResult { Success = false, Message = "Unauthorized session." };
        }

        if (request.FromSlot < 0 || request.FromSlot >= MaxInventorySlots ||
            request.ToSlot < 0 || request.ToSlot >= MaxInventorySlots)
        {
            return new InventoryOperationResult { Success = false, Message = "Invalid inventory slot index." };
        }

        if (request.FromSlot == request.ToSlot)
        {
            return new InventoryOperationResult { Success = true, Message = "Slots are identical." };
        }

        var swapped = await _inventoryRepository.SwapSlotsAsync(request.CharacterId, request.FromSlot, request.ToSlot);
        if (!swapped)
        {
            return new InventoryOperationResult { Success = false, Message = "Failed to swap inventory slots." };
        }

        var updatedItems = await _inventoryRepository.GetByCharacterIdAsync(request.CharacterId);
        return new InventoryOperationResult
        {
            Success = true,
            Message = "Slots swapped successfully.",
            InventoryItems = updatedItems
        };
    }

    public async Task<InventoryOperationResult> EquipItemAsync(EquipItemRequest request)
    {
        var session = await _sessionService.GetSessionAsync(request.SessionToken);
        if (session == null || session.ActiveCharacterId != request.CharacterId)
        {
            return new InventoryOperationResult { Success = false, Message = "Unauthorized session." };
        }

        var item = await _inventoryRepository.GetByInstanceIdAsync(request.ItemInstanceId);
        if (item == null || item.CharacterId != request.CharacterId)
        {
            return new InventoryOperationResult { Success = false, Message = "Item instance not found in character inventory." };
        }

        if (item.IsEquipped)
        {
            return new InventoryOperationResult { Success = false, Message = "Item is already equipped." };
        }

        var equipped = await _inventoryRepository.ToggleEquipStatusAsync(item.Id, isEquipped: true, equipSlot: request.EquipSlot);
        if (!equipped)
        {
            return new InventoryOperationResult { Success = false, Message = "Failed to equip item." };
        }

        item.IsEquipped = true;
        item.EquipSlot = request.EquipSlot;

        return new InventoryOperationResult
        {
            Success = true,
            Message = $"Item '{item.ItemId}' equipped to slot '{request.EquipSlot}'.",
            AffectedItem = item
        };
    }

    public async Task<InventoryOperationResult> UnequipItemAsync(string sessionToken, Guid characterId, Guid itemInstanceId, int targetSlot)
    {
        var session = await _sessionService.GetSessionAsync(sessionToken);
        if (session == null || session.ActiveCharacterId != characterId)
        {
            return new InventoryOperationResult { Success = false, Message = "Unauthorized session." };
        }

        var item = await _inventoryRepository.GetByInstanceIdAsync(itemInstanceId);
        if (item == null || item.CharacterId != characterId)
        {
            return new InventoryOperationResult { Success = false, Message = "Item instance not found." };
        }

        if (!item.IsEquipped)
        {
            return new InventoryOperationResult { Success = false, Message = "Item is not currently equipped." };
        }

        var unequipped = await _inventoryRepository.ToggleEquipStatusAsync(item.Id, isEquipped: false, equipSlot: null);
        if (!unequipped)
        {
            return new InventoryOperationResult { Success = false, Message = "Failed to unequip item." };
        }

        item.IsEquipped = false;
        item.EquipSlot = null;

        return new InventoryOperationResult
        {
            Success = true,
            Message = $"Item '{item.ItemId}' unequipped.",
            AffectedItem = item
        };
    }

    public async Task<InventoryOperationResult> ApplyDurabilityLossAsync(Guid itemInstanceId, int durabilityAmount)
    {
        var item = await _inventoryRepository.GetByInstanceIdAsync(itemInstanceId);
        if (item == null)
        {
            return new InventoryOperationResult { Success = false, Message = "Item instance not found." };
        }

        item.Durability = Math.Max(0, item.Durability - durabilityAmount);
        await _inventoryRepository.AddOrUpdateItemAsync(item);

        return new InventoryOperationResult
        {
            Success = true,
            Message = $"Durability updated. Remaining: {item.Durability}",
            AffectedItem = item
        };
    }
}
