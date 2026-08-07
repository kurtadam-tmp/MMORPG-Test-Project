using System;

namespace MMORPG.Shared.DTOs;

public class EnterDungeonRequest
{
    public string SessionToken { get; set; } = string.Empty;
    public Guid LeaderCharacterId { get; set; }
    public Guid PartyId { get; set; }
    public string DungeonTypeId { get; set; } = string.Empty;
}
