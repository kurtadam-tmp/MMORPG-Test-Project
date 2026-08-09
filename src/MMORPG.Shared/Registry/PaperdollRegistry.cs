using System.Collections.Generic;
using MMORPG.Shared.Enums;

namespace MMORPG.Shared.Registry;

public class PaperdollLayerInfo
{
    public EquipmentSlot Slot { get; set; }
    public int RenderOrder { get; set; }
    public string ItemId { get; set; } = string.Empty;
    public string TextureResourcePattern { get; set; } = string.Empty;
}

public static class PaperdollRegistry
{
    private static readonly Dictionary<string, PaperdollLayerInfo> _registry = new()
    {
        // Weapons
        ["IronSword"] = new PaperdollLayerInfo
        {
            ItemId = "IronSword",
            Slot = EquipmentSlot.MainHand,
            RenderOrder = 8,
            TextureResourcePattern = "res://Assets/Textures/Paperdoll/Weapons/IronSword/{dir}.png"
        },
        ["MagicStaff"] = new PaperdollLayerInfo
        {
            ItemId = "MagicStaff",
            Slot = EquipmentSlot.MainHand,
            RenderOrder = 8,
            TextureResourcePattern = "res://Assets/Textures/Paperdoll/Weapons/MagicStaff/{dir}.png"
        },

        // Armor (Chest)
        ["LeatherChest"] = new PaperdollLayerInfo
        {
            ItemId = "LeatherChest",
            Slot = EquipmentSlot.Chest,
            RenderOrder = 5,
            TextureResourcePattern = "res://Assets/Textures/Paperdoll/Armor/LeatherChest/{dir}.png"
        },
        ["IronPlateChest"] = new PaperdollLayerInfo
        {
            ItemId = "IronPlateChest",
            Slot = EquipmentSlot.Chest,
            RenderOrder = 5,
            TextureResourcePattern = "res://Assets/Textures/Paperdoll/Armor/IronPlateChest/{dir}.png"
        },

        // Leggings (Legs)
        ["IronLeggings"] = new PaperdollLayerInfo
        {
            ItemId = "IronLeggings",
            Slot = EquipmentSlot.Legs,
            RenderOrder = 3,
            TextureResourcePattern = "res://Assets/Textures/Paperdoll/Legs/IronLeggings/{dir}.png"
        },

        // Boots (Footwear)
        ["IronBoots"] = new PaperdollLayerInfo
        {
            ItemId = "IronBoots",
            Slot = EquipmentSlot.Boots,
            RenderOrder = 2,
            TextureResourcePattern = "res://Assets/Textures/Paperdoll/Boots/IronBoots/{dir}.png"
        },

        // Helmets (Head)
        ["IronHelm"] = new PaperdollLayerInfo
        {
            ItemId = "IronHelm",
            Slot = EquipmentSlot.Head,
            RenderOrder = 7,
            TextureResourcePattern = "res://Assets/Textures/Paperdoll/Head/IronHelm/{dir}.png"
        },

        // Shields (OffHand)
        ["TowerShield"] = new PaperdollLayerInfo
        {
            ItemId = "TowerShield",
            Slot = EquipmentSlot.OffHand,
            RenderOrder = 9,
            TextureResourcePattern = "res://Assets/Textures/Paperdoll/Shields/TowerShield/{dir}.png"
        }
    };

    public static PaperdollLayerInfo? GetLayerInfo(string itemId)
    {
        return _registry.TryGetValue(itemId, out var info) ? info : null;
    }
}
