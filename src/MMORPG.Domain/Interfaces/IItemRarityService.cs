using MMORPG.Domain.Enums;

namespace MMORPG.Domain.Interfaces;

public enum ItemCategory
{
    Equipment = 0,
    Weapon = 1,
    Armor = 2,
    Accessory = 3,
    Consumable = 4
}

public class GeneratedItem
{
    public Guid ItemId { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public ItemCategory Category { get; set; }
    public ItemRarity Rarity { get; set; }
    public int RequiredLevel { get; set; } = 1;
    public int CurrentDurability { get; set; } = 100;
    public int MaxDurability { get; set; } = 100;
    public int BaseArmor { get; set; }
    public int BaseDamage { get; set; }
    public Dictionary<string, int> StatAffixes { get; set; } = new();
    public string UniquePassiveSkill { get; set; } = string.Empty;
    public int Sockets { get; set; }
}

public interface IItemRarityService
{
    GeneratedItem GenerateRandomDrop(string baseItemName, ItemCategory category, int monsterLevel, float magicFindMultiplier);
    bool RepairEquipment(Guid itemId, out int goldCost);
    string FormatItemTooltip(GeneratedItem item);
}
