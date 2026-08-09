namespace MMORPG.Domain.Interfaces;

public class AdvancedQuestDefinition
{
    public string QuestId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int RequiredLevel { get; set; } = 1;
    public string TargetMobTypeId { get; set; } = string.Empty;
    public int TargetKillCount { get; set; } = 5;
    public long GoldReward { get; set; } = 350;
    public long ExpReward { get; set; } = 850;
    public string ItemRewardTemplateId { get; set; } = string.Empty;
}

public class PlayerQuestProgress
{
    public Guid CharacterId { get; set; }
    public string QuestId { get; set; } = string.Empty;
    public int CurrentKills { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsClaimed { get; set; }
}

public interface IAdvancedQuestService
{
    IEnumerable<AdvancedQuestDefinition> GetAllQuests();
    bool AcceptQuest(Guid characterId, string questId);
    bool RecordMobKill(Guid characterId, string mobTypeId, out bool questCompleted);
    bool ClaimQuestRewards(Guid characterId, string questId, out long goldReward, out long expReward, out string itemReward);
}
