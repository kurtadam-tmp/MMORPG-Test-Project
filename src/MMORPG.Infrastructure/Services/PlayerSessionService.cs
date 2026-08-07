using System.Collections.Concurrent;
using MMORPG.Domain.Interfaces;
using MMORPG.Domain.Models;

namespace MMORPG.Infrastructure.Services;

public class PlayerSessionService : IPlayerSessionService
{
    private readonly ICacheService _cacheService;
    private static readonly TimeSpan DefaultSessionTtl = TimeSpan.FromHours(24);
    private readonly ConcurrentDictionary<string, PlayerSession> _memorySessions = new();

    public PlayerSessionService(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }

    private static string GetKey(string token) => $"session:{token}";

    public async Task<PlayerSession> CreateSessionAsync(Guid playerId, string username)
    {
        var sessionToken = Guid.NewGuid().ToString("N");
        var session = new PlayerSession
        {
            SessionToken = sessionToken,
            PlayerId = playerId,
            Username = username,
            CreatedAt = DateTime.UtcNow,
            LastActivityAt = DateTime.UtcNow
        };

        _memorySessions.TryAdd(sessionToken, session);

        try
        {
            await _cacheService.SetAsync(GetKey(sessionToken), session, DefaultSessionTtl);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SessionNotice] Redis cache pending: {ex.Message}");
        }

        return session;
    }

    public async Task<PlayerSession?> GetSessionAsync(string sessionToken)
    {
        try
        {
            var session = await _cacheService.GetAsync<PlayerSession>(GetKey(sessionToken));
            if (session != null)
            {
                session.LastActivityAt = DateTime.UtcNow;
                _memorySessions[sessionToken] = session;
                return session;
            }
        }
        catch
        {
            // Redis offline fallback
        }

        _memorySessions.TryGetValue(sessionToken, out var memSession);
        return memSession;
    }

    public async Task<bool> UpdateActiveCharacterAsync(string sessionToken, Guid characterId)
    {
        var session = await GetSessionAsync(sessionToken);
        if (session == null) return false;

        session.ActiveCharacterId = characterId;
        session.LastActivityAt = DateTime.UtcNow;
        _memorySessions[sessionToken] = session;

        try
        {
            await _cacheService.SetAsync(GetKey(sessionToken), session, DefaultSessionTtl);
        }
        catch
        {
            // Redis offline fallback
        }

        return true;
    }

    public async Task<bool> RevokeSessionAsync(string sessionToken)
    {
        _memorySessions.TryRemove(sessionToken, out _);
        try
        {
            return await _cacheService.RemoveAsync(GetKey(sessionToken));
        }
        catch
        {
            return true;
        }
    }
}
