using System;

namespace MMORPG.Shared.DTOs;

public class MovementInputRequest
{
    public string SessionToken { get; set; } = string.Empty;
    public Guid CharacterId { get; set; }
    public float TargetX { get; set; }
    public float TargetY { get; set; }
    public float TargetZ { get; set; }
    public long ClientTimestampMs { get; set; }
    public long SequenceId { get; set; }
}
