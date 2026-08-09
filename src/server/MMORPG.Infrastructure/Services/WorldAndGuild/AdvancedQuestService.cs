using System.Collections.Concurrent;
using MMORPG.Domain.Interfaces;

namespace MMORPG.Infrastructure.Services;

public class AdvancedQuestService : IAdvancedQuestService
{
    private static readonly List<AdvancedQuestDefinition> QuestDatabase = new()
    {
        new AdvancedQuestDefinition
        {
            QuestId = "quest_goblin_slayer",
            Title = "Goblin Menace",
            Description = "Slay 5 Forest Goblins terrifying the grass plain village.",
            RequiredLevel = 1,
            TargetMobTypeId = "goblin",
            TargetKillCount = 5,
            GoldReward = 350,
            ExpReward = 850,
            ItemRewardTemplateId = "item_potion_hp"
        },
        new AdvancedQuestDefinition
        {
            QuestId = "quest_boss_ignis",
            Title = "Dragon Slayer: Ignis",
            Description = "Defeat World Boss Inferno Dragon Ignis in Volcano Arena!",
            RequiredLevel = 50,
            TargetMobTypeId = "boss_ignis",
            TargetKillCount = 1,
            GoldReward = 15000,
            ExpReward = 50000,
            ItemRewardTemplateId = "item_legendary_sword"
        }
    };

    private readonly ConcurrentDictionary<(Guid CharacterId, string QuestId), PlayerQuestProgress> _progress = new();

    public IEnumerable<AdvancedQuestDefinition> GetAllQuests() => QuestDatabase;

    public bool AcceptQuest(Guid characterId, string questId)
    {
        var key = (characterId, questId);
        if (_progress.ContainsKey(key)) return false;

        _progress[key] = new PlayerQuestProgress
        {
            CharacterId = characterId,
            QuestId = questId,
            CurrentKills = 0,
            IsCompleted = false
        };

        Console.WriteLine($"[QuestService] Character '{characterId}' accepted Quest '{questId}'!");
        return true;
    }

    public bool RecordMobKill(Guid characterId, string mobTypeId, out bool questCompleted)
    {
        questCompleted = false;
        foreach (var quest in QuestDatabase.Where(q => q.TargetMobTypeId.Equals(mobTypeId, StringComparison.OrdinalIgnoreCase)))
        {
            var key = (characterId, quest.QuestId);
            if (_progress.TryGetValue(key, out var prog) && !prog.IsCompleted)
            {
                prog.CurrentKills++;
                if (prog.CurrentKills >= quest.TargetKillCount)
                {
                    prog.IsCompleted = true;
                    questCompleted = true;
                    Console.WriteLine($"[QuestService COMPLETED] Character '{characterId}' COMPLETED Quest '{quest.Title}'!");
                }
                return true;
            }
        }
        return false;
    }

    public bool ClaimQuestRewards(Guid characterId, string questId, out long goldReward, out long expReward, out string itemReward)
    {
        goldReward = 0;
        expReward = 0;
        itemReward = string.Empty;

        var key = (characterId, questId);
        if (_progress.TryGetValue(key, out var prog) && prog.IsCompleted && !prog.IsClaimed)
        {
            var quest = QuestDatabase.FirstOrDefault(q => q.QuestId.Equals(questId, StringComparison.OrdinalIgnoreCase));
            if (quest != null)
            {
                prog.IsClaimed = true;
                goldReward = quest.GoldReward;
                expReward = quest.ExpReward;
                itemReward = quest.ItemRewardTemplateId;
                Console.WriteLine($"[QuestService CLAIMED] Character '{characterId}' claimed rewards for '{quest.Title}': +{goldReward} Gold, +{expReward} EXP, {itemReward}!");
                return true;
            }
        }

        return false;
    }
}
