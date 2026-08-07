using System.Collections.Concurrent;
using MMORPG.Domain.Enums;
using MMORPG.Domain.Interfaces;

namespace MMORPG.Infrastructure.Services;

public class ItemRarityService : IItemRarityService
{
    private readonly ConcurrentDictionary<Guid, GeneratedItem> _items = new();
    private static readonly Random _rng = new();

    public GeneratedItem GenerateRandomDrop(string baseItemName, ItemCategory category, int monsterLevel, float magicFindMultiplier)
    {
        ItemRarity rarity = RollRarity(magicFindMultiplier);
        int itemLevel = Math.Clamp(monsterLevel, 1, 60);
        int itemTier = CalculateItemTier(itemLevel);

        var item = new GeneratedItem
        {
            ItemId = Guid.NewGuid(),
            Name = BuildItemTierName(baseItemName, rarity, itemTier),
            Category = category,
            Rarity = rarity,
            RequiredLevel = itemLevel,
            CurrentDurability = 100 + (itemTier * 10),
            MaxDurability = 100 + (itemTier * 10),
            BaseArmor = category == ItemCategory.Equipment ? (itemTier * 15) + (itemLevel * 3) : 0,
            BaseDamage = category == ItemCategory.Equipment ? (itemTier * 25) + (itemLevel * 5) : 0,
            Sockets = (int)rarity >= 3 ? (int)rarity - 2 : 0, // Rare: 1, Epic: 2, Legendary: 3
            UniquePassiveSkill = category == ItemCategory.Equipment && rarity >= ItemRarity.Legendary ? "Lifesteal 5% & Chain Lightning on Hit" : string.Empty
        };

        // Roll Stat Affixes scaling with Item Tier & Rarity
        int numAffixes = (int)rarity;
        for (int i = 0; i < numAffixes; i++)
        {
            string statName = GetRandomStatName(i);
            int statVal = (itemTier * 10) + (itemLevel * 2) + _rng.Next(1, 10);
            item.StatAffixes[statName] = statVal;
        }

        _items[item.ItemId] = item;
        Console.WriteLine($"[ItemRarity TIER GENERATION] Generated T{itemTier} '{item.Name}' [{rarity}] (Req Level {itemLevel}, Base Dmg: {item.BaseDamage}, Base Armor: {item.BaseArmor}, Sockets: {item.Sockets})!");
        return item;
    }

    public bool RepairEquipment(Guid itemId, out int goldCost)
    {
        goldCost = 0;
        if (_items.TryGetValue(itemId, out var item))
        {
            int missingDurability = item.MaxDurability - item.CurrentDurability;
            if (missingDurability <= 0) return false;

            goldCost = missingDurability * 5;
            item.CurrentDurability = item.MaxDurability;
            Console.WriteLine($"[ItemRarity REPAIR] Item '{item.Name}' fully repaired! Cost: {goldCost} Gold.");
            return true;
        }
        return false;
    }

    public string FormatItemTooltip(GeneratedItem item)
    {
        int tier = CalculateItemTier(item.RequiredLevel);
        var statsFormatted = string.Join("\n", item.StatAffixes.Select(kv => $"  + {kv.Value} {kv.Key}"));
        return $"[T{tier} {item.Rarity.ToString().ToUpper()}] {item.Name}\nRequired Level: {item.RequiredLevel}\nDurability: {item.CurrentDurability}/{item.MaxDurability}\nBase Dmg: {item.BaseDamage} | Base Armor: {item.BaseArmor}\nSockets: {item.Sockets}\nStats:\n{statsFormatted}\nPassive: {item.UniquePassiveSkill}";
    }

    private int CalculateItemTier(int level)
    {
        if (level >= 60) return 7; // Tier 7 Max Level
        if (level >= 50) return 6; // Tier 6
        if (level >= 40) return 5; // Tier 5
        if (level >= 30) return 4; // Tier 4
        if (level >= 20) return 3; // Tier 3
        if (level >= 10) return 2; // Tier 2
        return 1;                  // Tier 1 (Novice)
    }

    private string BuildItemTierName(string baseName, ItemRarity rarity, int tier)
    {
        string tierPrefix = tier switch
        {
            1 => "Novice",
            2 => "Refined Steel",
            3 => "Mithril",
            4 => "Adamantite",
            5 => "Dragonscale",
            6 => "Obsidian",
            7 => "Godslayer",
            _ => "Standard"
        };

        string raritySuffix = rarity switch
        {
            ItemRarity.Uncommon => "of the Bear",
            ItemRarity.Rare => "of Precision",
            ItemRarity.Epic => "of the Storm",
            ItemRarity.Legendary => "of Destruction",
            ItemRarity.Mythic => "[Artifact Worldbreaker]",
            _ => string.Empty
        };

        return string.IsNullOrEmpty(raritySuffix) ? $"{tierPrefix} {baseName}" : $"{tierPrefix} {baseName} {raritySuffix}";
    }

    private ItemRarity RollRarity(float mf)
    {
        double roll = _rng.NextDouble() * 100 / Math.Max(1.0f, mf);
        if (roll < 0.5) return ItemRarity.Mythic;      // 0.5%
        if (roll < 3.0) return ItemRarity.Legendary;   // 2.5%
        if (roll < 10.0) return ItemRarity.Epic;       // 7.0%
        if (roll < 25.0) return ItemRarity.Rare;       // 15.0%
        if (roll < 55.0) return ItemRarity.Uncommon;   // 30.0%
        if (roll < 85.0) return ItemRarity.Common;     // 30.0%
        return ItemRarity.Poor;
    }

    private string GetRandomStatName(int index)
    {
        string[] stats = { "Strength", "Agility", "Intelligence", "Vitality", "Critical Chance %", "Attack Power", "Armor" };
        return stats[index % stats.Length];
    }
}
