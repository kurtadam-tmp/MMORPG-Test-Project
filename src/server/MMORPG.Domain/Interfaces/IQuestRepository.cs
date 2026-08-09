namespace MMORPG.Domain.Interfaces;

public class CharacterQuestProgress
{
    public Guid CharacterId { get; set; }
    public string QuestId { get; set; } = string.Empty;
    public int CurrentProgress { get; set; }
    public int TargetAmount { get; set; }
    public string Status { get; set; } = "IN_PROGRESS";
    public DateTime AcceptedAt { get; set; }
}

public interface IQuestRepository
{
    Task<CharacterQuestProgress?> GetQuestAsync(Guid characterId, string questId);
    Task<IEnumerable<CharacterQuestProgress>> GetActiveQuestsAsync(Guid characterId);
    Task<bool> AcceptQuestAsync(Guid characterId, string questId, int targetAmount);
    Task<bool> IncrementQuestProgressAsync(Guid characterId, string questId, int amount);
    Task<bool> ClaimQuestRewardAtomicAsync(Guid characterId, string questId, long rewardGold, long rewardXp);
}
