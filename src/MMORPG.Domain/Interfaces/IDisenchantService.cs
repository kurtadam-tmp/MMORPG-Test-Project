namespace MMORPG.Domain.Interfaces;

public class DisenchantResult
{
    public bool Success { get; set; }
    public List<(string MaterialId, int Amount)> ReagentsObtained { get; set; } = new();
    public string Message { get; set; } = string.Empty;
}

public interface IDisenchantService
{
    DisenchantResult DisenchantItem(Guid characterId, Guid itemInstanceId, string itemRarity);
}
