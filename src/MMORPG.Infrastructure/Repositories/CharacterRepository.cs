using System.Data;
using Dapper;
using MMORPG.Domain.Entities;
using MMORPG.Domain.Enums;
using MMORPG.Domain.Interfaces;
using MMORPG.Infrastructure.Data;

namespace MMORPG.Infrastructure.Repositories;

public class CharacterRepository : ICharacterRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public CharacterRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Character?> GetByIdAsync(Guid id)
    {
        using var db = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT id, player_id AS PlayerId, name, level, experience, 
                   character_class AS CharacterClass, pos_x AS PosX, pos_y AS PosY, pos_z AS PosZ, 
                   zone_id AS ZoneId, is_deleted AS IsDeleted, created_at AS CreatedAt, updated_at AS UpdatedAt
            FROM characters
            WHERE id = @Id AND is_deleted = FALSE;";

        return await db.QuerySingleOrDefaultAsync<Character>(sql, new { Id = id });
    }

    public async Task<IEnumerable<Character>> GetByPlayerIdAsync(Guid playerId)
    {
        using var db = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT id, player_id AS PlayerId, name, level, experience, 
                   character_class AS CharacterClass, pos_x AS PosX, pos_y AS PosY, pos_z AS PosZ, 
                   zone_id AS ZoneId, is_deleted AS IsDeleted, created_at AS CreatedAt, updated_at AS UpdatedAt
            FROM characters
            WHERE player_id = @PlayerId AND is_deleted = FALSE
            ORDER BY created_at ASC;";

        return await db.QueryAsync<Character>(sql, new { PlayerId = playerId });
    }

    public async Task<Guid> CreateWithStatsAsync(Character character, Stat stat)
    {
        using var db = _connectionFactory.CreateConnection();
        db.Open();
        using var transaction = db.BeginTransaction();

        try
        {
            const string insertCharSql = @"
                INSERT INTO characters (player_id, name, level, experience, character_class, pos_x, pos_y, pos_z, zone_id)
                VALUES (@PlayerId, @Name, @Level, @Experience, @CharacterClass, @PosX, @PosY, @PosZ, @ZoneId)
                RETURNING id;";

            var charId = await db.ExecuteScalarAsync<Guid>(insertCharSql, new
            {
                character.PlayerId,
                character.Name,
                character.Level,
                character.Experience,
                CharacterClass = character.CharacterClass.ToString().ToUpperInvariant(),
                character.PosX,
                character.PosY,
                character.PosZ,
                character.ZoneId
            }, transaction);

            const string insertStatSql = @"
                INSERT INTO stats (character_id, strength, agility, intelligence, vitality, current_hp, max_hp, current_mp, max_mp, unallocated_points)
                VALUES (@CharacterId, @Strength, @Agility, @Intelligence, @Vitality, @CurrentHp, @MaxHp, @CurrentMp, @MaxMp, @UnallocatedPoints);";

            await db.ExecuteAsync(insertStatSql, new
            {
                CharacterId = charId,
                stat.Strength,
                stat.Agility,
                stat.Intelligence,
                stat.Vitality,
                stat.CurrentHp,
                stat.MaxHp,
                stat.CurrentMp,
                stat.MaxMp,
                stat.UnallocatedPoints
            }, transaction);

            transaction.Commit();
            return charId;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<bool> UpdatePositionAsync(Guid characterId, float posX, float posY, float posZ, int zoneId)
    {
        using var db = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE characters
            SET pos_x = @PosX, pos_y = @PosY, pos_z = @PosZ, zone_id = @ZoneId
            WHERE id = @CharacterId;";

        var affected = await db.ExecuteAsync(sql, new { CharacterId = characterId, PosX = posX, PosY = posY, PosZ = posZ, ZoneId = zoneId });
        return affected > 0;
    }

    public async Task<bool> UpdateExperienceAndLevelAsync(Guid characterId, long experience, int level)
    {
        using var db = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE characters
            SET experience = @Experience, level = @Level
            WHERE id = @CharacterId;";

        var affected = await db.ExecuteAsync(sql, new { CharacterId = characterId, Experience = experience, Level = level });
        return affected > 0;
    }
}
