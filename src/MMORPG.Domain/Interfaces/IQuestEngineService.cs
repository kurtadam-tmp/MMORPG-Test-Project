using MMORPG.Shared.DTOs;

namespace MMORPG.Domain.Interfaces;

public interface IQuestEngineService
{
    Task<QuestResult> AcceptQuestAsync(AcceptQuestRequest request);
    Task<QuestResult> ClaimRewardAsync(CompleteQuestRequest request);
    Task RecordMobKillAsync(Guid characterId, string slainMobTypeId);
    Task<QuestResult> GetActiveQuestsAsync(string sessionToken, Guid characterId);
}
