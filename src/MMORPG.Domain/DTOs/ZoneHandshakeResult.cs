namespace MMORPG.Domain.DTOs;

public class ZoneHandshakeResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? SessionToken { get; set; }
    public CharacterDto? Character { get; set; }
    public float SpawnX { get; set; }
    public float SpawnY { get; set; }
    public float SpawnZ { get; set; }
}
