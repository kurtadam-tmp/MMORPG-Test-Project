using System.Data;
using Dapper;
using MMORPG.Domain.Entities;
using MMORPG.Domain.Interfaces;
using MMORPG.Infrastructure.Data;

namespace MMORPG.Infrastructure.Repositories;

public class InventoryRepository : IInventoryRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public InventoryRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<InventoryItem>> GetByCharacterIdAsync(Guid characterId)
    {
        using var db = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT id, instance_id AS InstanceId, character_id AS CharacterId, slot_index AS SlotIndex, 
                   item_id AS ItemId, quantity, durability, attributes::text AS AttributesJson, 
                   is_equipped AS IsEquipped, equip_slot AS EquipSlot, updated_at AS UpdatedAt
            FROM inventories
            WHERE character_id = @CharacterId
            ORDER BY slot_index ASC;";

        return await db.QueryAsync<InventoryItem>(sql, new { CharacterId = characterId });
    }

    public async Task<InventoryItem?> GetSlotAsync(Guid characterId, int slotIndex)
    {
        using var db = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT id, instance_id AS InstanceId, character_id AS CharacterId, slot_index AS SlotIndex, 
                   item_id AS ItemId, quantity, durability, attributes::text AS AttributesJson, 
                   is_equipped AS IsEquipped, equip_slot AS EquipSlot, updated_at AS UpdatedAt
            FROM inventories
            WHERE character_id = @CharacterId AND slot_index = @SlotIndex;";

        return await db.QuerySingleOrDefaultAsync<InventoryItem>(sql, new { CharacterId = characterId, SlotIndex = slotIndex });
    }

    public async Task<InventoryItem?> GetByInstanceIdAsync(Guid instanceId)
    {
        using var db = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT id, instance_id AS InstanceId, character_id AS CharacterId, slot_index AS SlotIndex, 
                   item_id AS ItemId, quantity, durability, attributes::text AS AttributesJson, 
                   is_equipped AS IsEquipped, equip_slot AS EquipSlot, updated_at AS UpdatedAt
            FROM inventories
            WHERE instance_id = @InstanceId;";

        return await db.QuerySingleOrDefaultAsync<InventoryItem>(sql, new { InstanceId = instanceId });
    }

    public async Task<bool> AddOrUpdateItemAsync(InventoryItem item)
    {
        using var db = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO inventories (instance_id, character_id, slot_index, item_id, quantity, durability, attributes, is_equipped, equip_slot)
            VALUES (@InstanceId, @CharacterId, @SlotIndex, @ItemId, @Quantity, @Durability, @AttributesJson::jsonb, @IsEquipped, @EquipSlot)
            ON CONFLICT (character_id, slot_index) DO UPDATE
            SET instance_id = EXCLUDED.instance_id,
                item_id = EXCLUDED.item_id,
                quantity = EXCLUDED.quantity,
                durability = EXCLUDED.durability,
                attributes = EXCLUDED.attributes,
                is_equipped = EXCLUDED.is_equipped,
                equip_slot = EXCLUDED.equip_slot;";

        var affected = await db.ExecuteAsync(sql, new
        {
            InstanceId = item.InstanceId == Guid.Empty ? Guid.NewGuid() : item.InstanceId,
            item.CharacterId,
            item.SlotIndex,
            item.ItemId,
            item.Quantity,
            item.Durability,
            item.AttributesJson,
            item.IsEquipped,
            item.EquipSlot
        });

        return affected > 0;
    }

    public async Task<bool> SwapSlotsAsync(Guid characterId, int fromSlot, int toSlot)
    {
        using var db = _connectionFactory.CreateConnection();
        db.Open();
        using var transaction = db.BeginTransaction();

        try
        {
            const string sql = @"
                UPDATE inventories
                SET slot_index = CASE 
                    WHEN slot_index = @FromSlot THEN @ToSlot
                    WHEN slot_index = @ToSlot THEN @FromSlot
                END
                WHERE character_id = @CharacterId AND slot_index IN (@FromSlot, @ToSlot);";

            var affected = await db.ExecuteAsync(sql, new { CharacterId = characterId, FromSlot = fromSlot, ToSlot = toSlot }, transaction);
            transaction.Commit();
            return affected > 0;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<bool> ToggleEquipStatusAsync(Guid itemId, bool isEquipped, string? equipSlot)
    {
        using var db = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE inventories
            SET is_equipped = @IsEquipped, equip_slot = @EquipSlot
            WHERE id = @ItemId;";

        var affected = await db.ExecuteAsync(sql, new { ItemId = itemId, IsEquipped = isEquipped, EquipSlot = equipSlot });
        return affected > 0;
    }
}
