using MMORPG.Domain.Interfaces;

namespace MMORPG.Infrastructure.Services;

public class LootEngineService : ILootEngineService
{
    public IEnumerable<LootDropResult> GenerateLootForMob(string mobTypeId, int mobLevel)
    {
        var drops = new List<LootDropResult>();
        float roll = Random.Shared.NextSingle();

        if (mobTypeId.StartsWith("boss_"))
        {
            // World Boss Loot Table (Guaranteed Legendary + Epic)
            drops.Add(new LootDropResult { ItemTemplateId = "item_legendary_sword", ItemName = "Ignis Fireblade", Rarity = "Legendary", Quantity = 1, GoldReward = 5000 });
            drops.Add(new LootDropResult { ItemTemplateId = "item_potion_elixir", ItemName = "Supreme Elixir (x10)", Rarity = "Epic", Quantity = 10, GoldReward = 1500 });
        }
        else
        {
            // Normal Mob Loot Table
            long gold = Random.Shared.Next(10, 50) * mobLevel;
            drops.Add(new LootDropResult { ItemTemplateId = "item_gold", ItemName = "Gold Coins", Rarity = "Common", Quantity = 1, GoldReward = gold });

            if (roll <= 0.25f) // 25% Chance for Rare Health Potion
            {
                drops.Add(new LootDropResult { ItemTemplateId = "item_potion_hp", ItemName = "Health Potion (L)", Rarity = "Rare", Quantity = 2, GoldReward = 0 });
            }
            if (roll <= 0.05f) // 5% Chance for Epic Ore
            {
                drops.Add(new LootDropResult { ItemTemplateId = "item_ore_mithril", ItemName = "Mithril Ore", Rarity = "Epic", Quantity = 1, GoldReward = 0 });
            }
        }

        return drops;
    }
}
