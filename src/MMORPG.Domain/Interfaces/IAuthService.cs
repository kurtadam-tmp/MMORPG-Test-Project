using MMORPG.Domain.DTOs;

namespace MMORPG.Domain.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterPlayerRequest request);
    Task<AuthResponse> LoginAsync(LoginPlayerRequest request);
}
