using Dapper;
using MMORPG.Domain.Entities;
using MMORPG.Domain.Interfaces;
using MMORPG.Infrastructure.Data;
using MMORPG.Shared.Enums;

namespace MMORPG.Infrastructure.Repositories;

public class GuildRepository : IGuildRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GuildRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Guild?> GetByIdAsync(Guid guildId)
    {
        using var db = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT id, name, leader_character_id AS LeaderCharacterId, 
                   vault_gold AS VaultGold, created_at AS CreatedAt
            FROM guilds
            WHERE id = @GuildId;";

        return await db.QuerySingleOrDefaultAsync<Guild>(sql, new { GuildId = guildId });
    }

    public async Task<Guild?> GetByNameAsync(string name)
    {
        using var db = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT id, name, leader_character_id AS LeaderCharacterId, 
                   vault_gold AS VaultGold, created_at AS CreatedAt
            FROM guilds
            WHERE LOWER(name) = LOWER(@Name);";

        return await db.QuerySingleOrDefaultAsync<Guild>(sql, new { Name = name });
    }

    public async Task<GuildMember?> GetMemberAsync(Guid characterId)
    {
        using var db = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT guild_id AS GuildId, character_id AS CharacterId, 
                   rank, joined_at AS JoinedAt
            FROM guild_members
            WHERE character_id = @CharacterId;";

        var row = await db.QuerySingleOrDefaultAsync<dynamic>(sql, new { CharacterId = characterId });
        if (row == null) return null;

        return new GuildMember
        {
            GuildId = row.guildid,
            CharacterId = row.characterid,
            Rank = Enum.Parse<GuildRank>((string)row.rank, ignoreCase: true),
            JoinedAt = row.joinedat
        };
    }

    public async Task<IEnumerable<GuildMember>> GetGuildMembersAsync(Guid guildId)
    {
        using var db = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT guild_id AS GuildId, character_id AS CharacterId, 
                   rank, joined_at AS JoinedAt
            FROM guild_members
            WHERE guild_id = @GuildId;";

        var rows = await db.QueryAsync<dynamic>(sql, new { GuildId = guildId });
        return rows.Select(r => new GuildMember
        {
            GuildId = r.guildid,
            CharacterId = r.characterid,
            Rank = Enum.Parse<GuildRank>((string)r.rank, ignoreCase: true),
            JoinedAt = r.joinedat
        });
    }

    public async Task<bool> CreateGuildAtomicAsync(Guild guild, GuildMember leaderMember)
    {
        using var db = _connectionFactory.CreateConnection();
        db.Open();
        using var tx = db.BeginTransaction();

        try
        {
            const string insertGuildSql = @"
                INSERT INTO guilds (name, leader_character_id, vault_gold)
                VALUES (@Name, @LeaderCharacterId, 0)
                RETURNING id;";

            var guildId = await db.ExecuteScalarAsync<Guid>(insertGuildSql, guild, tx);
            guild.Id = guildId;
            leaderMember.GuildId = guildId;

            const string insertMemberSql = @"
                INSERT INTO guild_members (guild_id, character_id, rank)
                VALUES (@GuildId, @CharacterId, @Rank);";

            await db.ExecuteAsync(insertMemberSql, new
            {
                GuildId = guildId,
                CharacterId = leaderMember.CharacterId,
                Rank = leaderMember.Rank.ToString().ToUpper()
            }, tx);

            tx.Commit();
            return true;
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    public async Task<bool> AddMemberAsync(GuildMember member)
    {
        using var db = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO guild_members (guild_id, character_id, rank)
            VALUES (@GuildId, @CharacterId, @Rank);";

        var affected = await db.ExecuteAsync(sql, new
        {
            member.GuildId,
            member.CharacterId,
            Rank = member.Rank.ToString().ToUpper()
        });

        return affected > 0;
    }

    public async Task<bool> RemoveMemberAsync(Guid guildId, Guid characterId)
    {
        using var db = _connectionFactory.CreateConnection();
        const string sql = @"
            DELETE FROM guild_members
            WHERE guild_id = @GuildId AND character_id = @CharacterId;";

        var affected = await db.ExecuteAsync(sql, new { GuildId = guildId, CharacterId = characterId });
        return affected > 0;
    }

    public async Task<bool> UpdateMemberRankAsync(Guid guildId, Guid characterId, GuildRank newRank)
    {
        using var db = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE guild_members
            SET rank = @Rank
            WHERE guild_id = @GuildId AND character_id = @CharacterId;";

        var affected = await db.ExecuteAsync(sql, new
        {
            GuildId = guildId,
            CharacterId = characterId,
            Rank = newRank.ToString().ToUpper()
        });

        return affected > 0;
    }

    public async Task<bool> DepositVaultGoldAtomicAsync(Guid guildId, Guid characterId, long goldAmount)
    {
        using var db = _connectionFactory.CreateConnection();
        db.Open();
        using var tx = db.BeginTransaction();

        try
        {
            // 1. Deduct gold from character stats
            const string deductGoldSql = @"
                UPDATE stats
                SET gold = gold - @GoldAmount
                WHERE character_id = @CharacterId AND gold >= @GoldAmount;";

            var deducted = await db.ExecuteAsync(deductGoldSql, new { CharacterId = characterId, GoldAmount = goldAmount }, tx);
            if (deducted == 0)
            {
                tx.Rollback();
                return false;
            }

            // 2. Add gold to guild vault
            const string addVaultSql = @"
                UPDATE guilds
                SET vault_gold = vault_gold + @GoldAmount
                WHERE id = @GuildId;";

            await db.ExecuteAsync(addVaultSql, new { GuildId = guildId, GoldAmount = goldAmount }, tx);

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
