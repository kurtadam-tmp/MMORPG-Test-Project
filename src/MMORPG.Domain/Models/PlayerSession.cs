namespace MMORPG.Domain.Models;

public class PlayerSession
{
    public string SessionToken { get; set; } = string.Empty;
    public Guid PlayerId { get; set; }
    public string Username { get; set; } = string.Empty;
    public Guid? ActiveCharacterId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastActivityAt { get; set; } = DateTime.UtcNow;
}
