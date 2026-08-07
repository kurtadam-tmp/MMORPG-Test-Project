using MMORPG.Domain.Interfaces;

namespace MMORPG.Infrastructure.Services;

public class CraftingEngineService : ICraftingEngineService
{
    private static readonly List<AdvancedCraftingRecipe> MasterRecipes = new()
    {
        new AdvancedCraftingRecipe
        {
            RecipeId = "craft_dragon_slayer",
            Name = "Dragon Slayer Greatsword",
            Category = "Blacksmithing",
            RequiredLevel = 50,
            SuccessChance = 0.85f,
            Materials = new() { ("item_dragon_scale", 5), ("item_mithril_bar", 10), ("item_lava_core", 2) },
            ResultItemId = "item_sword_dragonslayer",
            ResultRarity = "Legendary"
        },
        new AdvancedCraftingRecipe
        {
            RecipeId = "craft_archmage_robe",
            Name = "Archmage Arcane Robe",
            Category = "Tailoring",
            RequiredLevel = 45,
            SuccessChance = 0.90f,
            Materials = new() { ("item_silk_cloth", 15), ("item_arcane_dust", 8) },
            ResultItemId = "item_armor_archmage",
            ResultRarity = "Epic"
        },
        new AdvancedCraftingRecipe
        {
            RecipeId = "craft_elixir_power",
            Name = "Elixir of Ultimate Power",
            Category = "Alchemy",
            RequiredLevel = 30,
            SuccessChance = 1.00f,
            Materials = new() { ("item_lava_flower", 3), ("item_crystal_vial", 1) },
            ResultItemId = "item_potion_elixir",
            ResultRarity = "Rare"
        }
    };

    public IEnumerable<AdvancedCraftingRecipe> GetAllRecipes() => MasterRecipes;

    public IEnumerable<AdvancedCraftingRecipe> GetRecipesByCategory(string category)
    {
        return MasterRecipes.Where(r => r.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
    }

    public bool CraftItemAdvanced(Guid characterId, string recipeId, out string craftedItemName, out string rarity)
    {
        var recipe = MasterRecipes.FirstOrDefault(r => r.RecipeId.Equals(recipeId, StringComparison.OrdinalIgnoreCase));
        if (recipe == null)
        {
            craftedItemName = string.Empty;
            rarity = string.Empty;
            return false;
        }

        float roll = Random.Shared.NextSingle();
        if (roll <= recipe.SuccessChance)
        {
            craftedItemName = recipe.Name;
            rarity = recipe.ResultRarity;
            Console.WriteLine($"[Crafting SUCCESS] Character '{characterId}' successfully crafted '{recipe.Name}' ({recipe.ResultRarity})!");
            return true;
        }

        craftedItemName = string.Empty;
        rarity = string.Empty;
        Console.WriteLine($"[Crafting FAILED] Character '{characterId}' failed to craft '{recipe.Name}'. Materials consumed.");
        return false;
    }
}
