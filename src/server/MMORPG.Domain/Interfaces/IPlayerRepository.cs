using MMORPG.Domain.Entities;

namespace MMORPG.Domain.Interfaces;

public interface IPlayerRepository
{
    Task<Player?> GetByIdAsync(Guid id);
    Task<Player?> GetByUsernameAsync(string username);
    Task<Player?> GetByEmailAsync(string email);
    Task<Guid> CreateAsync(Player player);
    Task<bool> UpdateLastLoginAsync(Guid id);
}
