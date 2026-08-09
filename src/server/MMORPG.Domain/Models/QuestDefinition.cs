namespace MMORPG.Domain.Models;

public class QuestDefinition
{
    public string QuestId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string TargetMobTypeId { get; set; } = string.Empty;
    public int TargetAmount { get; set; }
    public long RewardGold { get; set; }
    public long RewardExperience { get; set; }
}
