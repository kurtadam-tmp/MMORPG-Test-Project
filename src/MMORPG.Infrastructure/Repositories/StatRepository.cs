using System.Data;
using Dapper;
using MMORPG.Domain.Entities;
using MMORPG.Domain.Interfaces;
using MMORPG.Infrastructure.Data;

namespace MMORPG.Infrastructure.Repositories;

public class StatRepository : IStatRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public StatRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Stat?> GetByCharacterIdAsync(Guid characterId)
    {
        using var db = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT character_id AS CharacterId, strength, agility, intelligence, vitality,
                   current_hp AS CurrentHp, max_hp AS MaxHp, current_mp AS CurrentMp, max_mp AS MaxMp,
                   unallocated_points AS UnallocatedPoints, gold, updated_at AS UpdatedAt
            FROM stats
            WHERE character_id = @CharacterId;";

        return await db.QuerySingleOrDefaultAsync<Stat>(sql, new { CharacterId = characterId });
    }

    public async Task<bool> UpdateAsync(Stat stat)
    {
        using var db = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE stats
            SET strength = @Strength, agility = @Agility, intelligence = @Intelligence, vitality = @Vitality,
                current_hp = @CurrentHp, max_hp = @MaxHp, current_mp = @CurrentMp, max_mp = @MaxMp,
                unallocated_points = @UnallocatedPoints, gold = @Gold
            WHERE character_id = @CharacterId;";

        var affected = await db.ExecuteAsync(sql, stat);
        return affected > 0;
    }

    public async Task<bool> UpdateGoldAsync(Guid characterId, long newGoldAmount)
    {
        using var db = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE stats
            SET gold = @Gold
            WHERE character_id = @CharacterId;";

        var affected = await db.ExecuteAsync(sql, new { CharacterId = characterId, Gold = newGoldAmount });
        return affected > 0;
    }

    public async Task<bool> AllocatePointsAsync(Guid characterId, int strDiff, int agiDiff, int intDiff, int vitDiff)
    {
        var totalCost = strDiff + agiDiff + intDiff + vitDiff;
        if (totalCost <= 0) return false;

        using var db = _connectionFactory.CreateConnection();
        db.Open();
        using var transaction = db.BeginTransaction();

        try
        {
            // Atomically check unallocated_points and update stats
            const string sql = @"
                UPDATE stats
                SET strength = strength + @StrDiff,
                    agility = agility + @AgiDiff,
                    intelligence = intelligence + @IntDiff,
                    vitality = vitality + @VitDiff,
                    unallocated_points = unallocated_points - @TotalCost
                WHERE character_id = @CharacterId AND unallocated_points >= @TotalCost;";

            var affected = await db.ExecuteAsync(sql, new
            {
                CharacterId = characterId,
                StrDiff = strDiff,
                AgiDiff = agiDiff,
                IntDiff = intDiff,
                VitDiff = vitDiff,
                TotalCost = totalCost
            }, transaction);

            if (affected > 0)
            {
                transaction.Commit();
                return true;
            }

            transaction.Rollback();
            return false;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}
