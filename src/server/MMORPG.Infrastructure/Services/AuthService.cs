using MMORPG.Domain.DTOs;
using MMORPG.Domain.Entities;
using MMORPG.Domain.Enums;
using MMORPG.Domain.Interfaces;

namespace MMORPG.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IPlayerRepository _playerRepository;
    private readonly IPlayerSessionService _sessionService;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AuthService(
        IPlayerRepository playerRepository,
        IPlayerSessionService sessionService,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _playerRepository = playerRepository;
        _sessionService = sessionService;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterPlayerRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || request.Username.Length < 3)
        {
            return new AuthResponse { Success = false, Message = "Username must be at least 3 characters long." };
        }

        if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains("@"))
        {
            return new AuthResponse { Success = false, Message = "Invalid email address." };
        }

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
        {
            return new AuthResponse { Success = false, Message = "Password must be at least 6 characters long." };
        }

        var existingUser = await _playerRepository.GetByUsernameAsync(request.Username);
        if (existingUser != null)
        {
            return new AuthResponse { Success = false, Message = "Username already taken." };
        }

        var existingEmail = await _playerRepository.GetByEmailAsync(request.Email);
        if (existingEmail != null)
        {
            return new AuthResponse { Success = false, Message = "Email address already registered." };
        }

        // Secure password hashing with BCrypt
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var player = new Player
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = passwordHash,
            Status = PlayerStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        var playerId = await _playerRepository.CreateAsync(player);

        // Generate Signed JWT Token
        var jwtToken = _jwtTokenGenerator.GenerateToken(playerId, player.Username);

        // Create active session in Redis
        var session = await _sessionService.CreateSessionAsync(playerId, player.Username);

        return new AuthResponse
        {
            Success = true,
            Message = "Player account registered successfully.",
            SessionToken = jwtToken,
            PlayerId = playerId,
            Username = player.Username
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginPlayerRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.UsernameOrEmail) || string.IsNullOrWhiteSpace(request.Password))
        {
            return new AuthResponse { Success = false, Message = "Username/Email and Password are required." };
        }

        var player = await _playerRepository.GetByUsernameAsync(request.UsernameOrEmail)
                     ?? await _playerRepository.GetByEmailAsync(request.UsernameOrEmail);

        if (player == null)
        {
            return new AuthResponse { Success = false, Message = "Invalid credentials." };
        }

        if (player.Status != PlayerStatus.Active)
        {
            return new AuthResponse { Success = false, Message = $"Account is {player.Status}." };
        }

        var isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, player.PasswordHash);
        if (!isPasswordValid)
        {
            return new AuthResponse { Success = false, Message = "Invalid credentials." };
        }

        await _playerRepository.UpdateLastLoginAsync(player.Id);

        // Generate Signed JWT Token
        var jwtToken = _jwtTokenGenerator.GenerateToken(player.Id, player.Username);

        // Create active session in Redis
        await _sessionService.CreateSessionAsync(player.Id, player.Username);

        return new AuthResponse
        {
            Success = true,
            Message = "Login successful.",
            SessionToken = jwtToken,
            PlayerId = player.Id,
            Username = player.Username
        };
    }
}
