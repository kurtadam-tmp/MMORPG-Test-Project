namespace MMORPG.Domain.Interfaces;

public class EnhancementResult
{
    public bool Success { get; set; }
    public int NewEnhancementLevel { get; set; }
    public bool ItemDestroyed { get; set; }
    public string Message { get; set; } = string.Empty;
}

public interface IItemEnhancementService
{
    EnhancementResult EnhanceItem(Guid characterId, Guid itemInstanceId, int currentLevel, bool useProtectionScroll);
    int CalculateBonusStatForLevel(int baseStat, int enhancementLevel);
}
