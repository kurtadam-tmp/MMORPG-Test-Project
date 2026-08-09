using System;

namespace MMORPG.Shared.DTOs;

public class CastSkillRequest
{
    public string SessionToken { get; set; } = string.Empty;
    public Guid AttackerCharacterId { get; set; }
    public Guid TargetCharacterId { get; set; }
    public string SkillId { get; set; } = string.Empty;
    public float TargetX { get; set; }
    public float TargetY { get; set; }
    public float TargetZ { get; set; }
}
