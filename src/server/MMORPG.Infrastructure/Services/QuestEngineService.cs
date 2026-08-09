using MMORPG.Domain.Interfaces;
using MMORPG.Domain.Models;
using MMORPG.Shared.DTOs;

namespace MMORPG.Infrastructure.Services;

public class QuestEngineService : IQuestEngineService
{
    private readonly IQuestRepository _questRepository;
    private readonly IPlayerSessionService _sessionService;

    private static readonly Dictionary<string, QuestDefinition> QuestDatabase = new()
    {
        ["q_goblin_hunter"] = new QuestDefinition
        {
            QuestId = "q_goblin_hunter",
            Title = "Goblin Menace",
            Description = "Slay 5 Forest Goblins threatening the local town.",
            TargetMobTypeId = "goblin",
            TargetAmount = 5,
            RewardGold = 150,
            RewardExperience = 300
        },
        ["q_wolf_pack"] = new QuestDefinition
        {
            QuestId = "q_wolf_pack",
            Title = "Wolf Extermination",
            Description = "Clear 3 Dire Wolves near the forest path.",
            TargetMobTypeId = "wolf",
            TargetAmount = 3,
            RewardGold = 250,
            RewardExperience = 500
        }
    };

    public QuestEngineService(
        IQuestRepository questRepository,
        IPlayerSessionService sessionService)
    {
        _questRepository = questRepository;
        _sessionService = sessionService;
    }

    public async Task<QuestResult> AcceptQuestAsync(AcceptQuestRequest request)
    {
        var session = await _sessionService.GetSessionAsync(request.SessionToken);
        if (session == null || session.ActiveCharacterId != request.CharacterId)
        {
            return new QuestResult { Success = false, Message = "Unauthorized session token." };
        }

        if (!QuestDatabase.TryGetValue(request.QuestId, out var questDef))
        {
            return new QuestResult { Success = false, Message = "Quest definition not found." };
        }

        var existing = await _questRepository.GetQuestAsync(request.CharacterId, request.QuestId);
        if (existing != null)
        {
            return new QuestResult { Success = false, Message = "Quest has already been accepted." };
        }

        await _questRepository.AcceptQuestAsync(request.CharacterId, request.QuestId, questDef.TargetAmount);

        return new QuestResult
        {
            Success = true,
            Message = $"Accepted quest '{questDef.Title}'!",
            Quest = questDef
        };
    }

    public async Task RecordMobKillAsync(Guid characterId, string slainMobTypeId)
    {
        var activeQuests = await _questRepository.GetActiveQuestsAsync(characterId);

        foreach (var quest in activeQuests)
        {
            if (quest.Status != "IN_PROGRESS") continue;

            if (QuestDatabase.TryGetValue(quest.QuestId, out var def) && def.TargetMobTypeId == slainMobTypeId)
            {
                await _questRepository.IncrementQuestProgressAsync(characterId, quest.QuestId, 1);
                Console.WriteLine($"[QuestEngine] Character '{characterId}' progress on '{def.Title}': {quest.CurrentProgress + 1}/{def.TargetAmount}");
            }
        }
    }

    public async Task<QuestResult> ClaimRewardAsync(CompleteQuestRequest request)
    {
        var session = await _sessionService.GetSessionAsync(request.SessionToken);
        if (session == null || session.ActiveCharacterId != request.CharacterId)
        {
            return new QuestResult { Success = false, Message = "Unauthorized session token." };
        }

        if (!QuestDatabase.TryGetValue(request.QuestId, out var questDef))
        {
            return new QuestResult { Success = false, Message = "Quest definition not found." };
        }

        var questProgress = await _questRepository.GetQuestAsync(request.CharacterId, request.QuestId);
        if (questProgress == null || questProgress.Status != "COMPLETED")
        {
            return new QuestResult { Success = false, Message = "Quest objectives not yet completed." };
        }

        var claimed = await _questRepository.ClaimQuestRewardAtomicAsync(
            request.CharacterId, 
            request.QuestId, 
            questDef.RewardGold, 
            questDef.RewardExperience);

        if (!claimed)
        {
            return new QuestResult { Success = false, Message = "Failed to claim reward or already claimed." };
        }

        return new QuestResult
        {
            Success = true,
            Message = $"Quest completed! Awarded {questDef.RewardGold} Gold & {questDef.RewardExperience} XP.",
            Quest = questDef
        };
    }

    public async Task<QuestResult> GetActiveQuestsAsync(string sessionToken, Guid characterId)
    {
        var session = await _sessionService.GetSessionAsync(sessionToken);
        if (session == null || session.ActiveCharacterId != characterId)
        {
            return new QuestResult { Success = false, Message = "Unauthorized session token." };
        }

        var activeQuests = await _questRepository.GetActiveQuestsAsync(characterId);
        return new QuestResult
        {
            Success = true,
            Message = "Active quests retrieved.",
            ActiveQuests = activeQuests
        };
    }
}
