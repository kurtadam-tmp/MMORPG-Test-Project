namespace MMORPG.Domain.DTOs;

public class ZoneHandoffToken
{
    public string Token { get; set; } = string.Empty;
    public Guid PlayerId { get; set; }
    public Guid CharacterId { get; set; }
    public int TargetZoneId { get; set; }
    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddMinutes(1);
}
