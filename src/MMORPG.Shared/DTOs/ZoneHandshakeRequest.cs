namespace MMORPG.Shared.DTOs;

public class ZoneHandshakeRequest
{
    public string HandoffToken { get; set; } = string.Empty;
    public int TargetZoneId { get; set; }
}
