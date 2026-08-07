namespace MMORPG.Domain.DTOs;

public class AuthResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? SessionToken { get; set; }
    public Guid? PlayerId { get; set; }
    public string? Username { get; set; }
}
