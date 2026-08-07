using Dapper;
using MMORPG.Domain.Interfaces;
using MMORPG.Infrastructure.Data;

namespace MMORPG.Infrastructure.Repositories;

public class QuestRepository : IQuestRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public QuestRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<CharacterQuestProgress?> GetQuestAsync(Guid characterId, string questId)
    {
        using var db = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT character_id AS CharacterId, quest_id AS QuestId, 
                   current_progress AS CurrentProgress, target_amount AS TargetAmount, 
                   status, accepted_at AS AcceptedAt
            FROM character_quests
            WHERE character_id = @CharacterId AND quest_id = @QuestId;";

        return await db.QuerySingleOrDefaultAsync<CharacterQuestProgress>(sql, new { CharacterId = characterId, QuestId = questId });
    }

    public async Task<IEnumerable<CharacterQuestProgress>> GetActiveQuestsAsync(Guid characterId)
    {
        using var db = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT character_id AS CharacterId, quest_id AS QuestId, 
                   current_progress AS CurrentProgress, target_amount AS TargetAmount, 
                   status, accepted_at AS AcceptedAt
            FROM character_quests
            WHERE character_id = @CharacterId AND status != 'REWARDED';";

        return await db.QueryAsync<CharacterQuestProgress>(sql, new { CharacterId = characterId });
    }

    public async Task<bool> AcceptQuestAsync(Guid characterId, string questId, int targetAmount)
    {
        using var db = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO character_quests (character_id, quest_id, current_progress, target_amount, status)
            VALUES (@CharacterId, @QuestId, 0, @TargetAmount, 'IN_PROGRESS')
            ON CONFLICT (character_id, quest_id) DO NOTHING;";

        var affected = await db.ExecuteAsync(sql, new { CharacterId = characterId, QuestId = questId, TargetAmount = targetAmount });
        return affected > 0;
    }

    public async Task<bool> IncrementQuestProgressAsync(Guid characterId, string questId, int amount)
    {
        using var db = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE character_quests
            SET current_progress = LEAST(target_amount, current_progress + @Amount),
                status = CASE WHEN current_progress + @Amount >= target_amount THEN 'COMPLETED' ELSE 'IN_PROGRESS' END
            WHERE character_id = @CharacterId AND quest_id = @QuestId AND status = 'IN_PROGRESS';";

        var affected = await db.ExecuteAsync(sql, new { CharacterId = characterId, QuestId = questId, Amount = amount });
        return affected > 0;
    }

    public async Task<bool> ClaimQuestRewardAtomicAsync(Guid characterId, string questId, long rewardGold, long rewardXp)
    {
        using var db = _connectionFactory.CreateConnection();
        db.Open();
        using var tx = db.BeginTransaction();

        try
        {
            // 1. Check and mark quest status as REWARDED
            const string markQuestSql = @"
                UPDATE character_quests
                SET status = 'REWARDED'
                WHERE character_id = @CharacterId AND quest_id = @QuestId AND status = 'COMPLETED';";

            var marked = await db.ExecuteAsync(markQuestSql, new { CharacterId = characterId, QuestId = questId }, tx);
            if (marked == 0)
            {
                tx.Rollback();
                return false;
            }

            // 2. Add Gold to stats table
            const string addGoldSql = @"
                UPDATE stats
                SET gold = gold + @RewardGold
                WHERE character_id = @CharacterId;";

            await db.ExecuteAsync(addGoldSql, new { CharacterId = characterId, RewardGold = rewardGold }, tx);

            // 3. Add XP to characters table
            const string addXpSql = @"
                UPDATE characters
                SET experience = experience + @RewardXp
                WHERE id = @CharacterId;";

            await db.ExecuteAsync(addXpSql, new { CharacterId = characterId, RewardXp = rewardXp }, tx);

            tx.Commit();
            return true;
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }
}
