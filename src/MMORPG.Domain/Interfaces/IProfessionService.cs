namespace MMORPG.Domain.Interfaces;

public class CraftingRecipe
{
    public string RecipeId { get; set; } = string.Empty;
    public string RecipeName { get; set; } = string.Empty;
    public string ProfessionType { get; set; } = string.Empty;
    public int RequiredProfessionLevel { get; set; } = 1;
    public List<(string IngredientId, int Amount)> RequiredIngredients { get; set; } = new();
    public (string ItemId, int Amount) ResultItem { get; set; }
}

public interface IProfessionService
{
    IEnumerable<CraftingRecipe> GetRecipesForProfession(string professionType);
    bool CraftItem(Guid characterId, string recipeId);
}
