using MMORPG.Domain.Interfaces;

namespace MMORPG.Infrastructure.Services;

public class ProfessionService : IProfessionService
{
    private static readonly List<CraftingRecipe> Recipes = new()
    {
        new CraftingRecipe
        {
            RecipeId = "recipe_iron_sword",
            RecipeName = "Iron Sword +5",
            ProfessionType = "Blacksmithing",
            RequiredProfessionLevel = 1,
            RequiredIngredients = new() { ("item_iron_ore", 3), ("item_wood", 2) },
            ResultItem = ("item_sword_01", 1)
        },
        new CraftingRecipe
        {
            RecipeId = "recipe_health_potion",
            RecipeName = "Health Potion (L)",
            ProfessionType = "Alchemy",
            RequiredProfessionLevel = 1,
            RequiredIngredients = new() { ("item_herb_peacebloom", 2), ("item_vial", 1) },
            ResultItem = ("item_potion_hp", 3)
        }
    };

    public IEnumerable<CraftingRecipe> GetRecipesForProfession(string professionType)
    {
        return Recipes.Where(r => r.ProfessionType.Equals(professionType, StringComparison.OrdinalIgnoreCase));
    }

    public bool CraftItem(Guid characterId, string recipeId)
    {
        var recipe = Recipes.FirstOrDefault(r => r.RecipeId.Equals(recipeId, StringComparison.OrdinalIgnoreCase));
        if (recipe == null) return false;

        Console.WriteLine($"[ProfessionService] Character '{characterId}' crafted '{recipe.RecipeName}' via {recipe.ProfessionType}!");
        return true;
    }
}
