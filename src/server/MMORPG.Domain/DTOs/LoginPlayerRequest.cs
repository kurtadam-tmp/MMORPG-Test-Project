namespace MMORPG.Domain.DTOs;

public class LoginPlayerRequest
{
    public string UsernameOrEmail { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
