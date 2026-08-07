using MMORPG.Domain.Models;

namespace MMORPG.Domain.Interfaces;

public interface IPlayerSessionService
{
    Task<PlayerSession> CreateSessionAsync(Guid playerId, string username);
    Task<PlayerSession?> GetSessionAsync(string sessionToken);
    Task<bool> UpdateActiveCharacterAsync(string sessionToken, Guid characterId);
    Task<bool> RevokeSessionAsync(string sessionToken);
}
