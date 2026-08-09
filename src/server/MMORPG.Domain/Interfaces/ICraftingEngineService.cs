namespace MMORPG.Domain.Interfaces;

public class AdvancedCraftingRecipe
{
    public string RecipeId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int RequiredLevel { get; set; } = 1;
    public float SuccessChance { get; set; } = 1.0f;
    public List<(string MaterialId, int Count)> Materials { get; set; } = new();
    public string ResultItemId { get; set; } = string.Empty;
    public string ResultRarity { get; set; } = "Common";
}

public interface ICraftingEngineService
{
    IEnumerable<AdvancedCraftingRecipe> GetAllRecipes();
    IEnumerable<AdvancedCraftingRecipe> GetRecipesByCategory(string category);
    bool CraftItemAdvanced(Guid characterId, string recipeId, out string craftedItemName, out string rarity);
}
