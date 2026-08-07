using System.Data;
using Dapper;
using MMORPG.Domain.Entities;
using MMORPG.Domain.Enums;
using MMORPG.Domain.Interfaces;
using MMORPG.Infrastructure.Data;

namespace MMORPG.Infrastructure.Repositories;

public class PlayerRepository : IPlayerRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public PlayerRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Player?> GetByIdAsync(Guid id)
    {
        using var db = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT id, username, email, password_hash AS PasswordHash, 
                   status, created_at AS CreatedAt, last_login_at AS LastLoginAt
            FROM players
            WHERE id = @Id;";

        return await db.QuerySingleOrDefaultAsync<Player>(sql, new { Id = id });
    }

    public async Task<Player?> GetByUsernameAsync(string username)
    {
        using var db = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT id, username, email, password_hash AS PasswordHash, 
                   status, created_at AS CreatedAt, last_login_at AS LastLoginAt
            FROM players
            WHERE LOWER(username) = LOWER(@Username);";

        return await db.QuerySingleOrDefaultAsync<Player>(sql, new { Username = username });
    }

    public async Task<Player?> GetByEmailAsync(string email)
    {
        using var db = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT id, username, email, password_hash AS PasswordHash, 
                   status, created_at AS CreatedAt, last_login_at AS LastLoginAt
            FROM players
            WHERE LOWER(email) = LOWER(@Email);";

        return await db.QuerySingleOrDefaultAsync<Player>(sql, new { Email = email });
    }

    public async Task<Guid> CreateAsync(Player player)
    {
        using var db = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO players (username, email, password_hash, status)
            VALUES (@Username, @Email, @PasswordHash, @Status::varchar)
            RETURNING id;";

        return await db.ExecuteScalarAsync<Guid>(sql, new
        {
            player.Username,
            player.Email,
            player.PasswordHash,
            Status = player.Status.ToString().ToUpperInvariant()
        });
    }

    public async Task<bool> UpdateLastLoginAsync(Guid id)
    {
        using var db = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE players 
            SET last_login_at = CURRENT_TIMESTAMP
            WHERE id = @Id;";

        var rows = await db.ExecuteAsync(sql, new { Id = id });
        return rows > 0;
    }
}
