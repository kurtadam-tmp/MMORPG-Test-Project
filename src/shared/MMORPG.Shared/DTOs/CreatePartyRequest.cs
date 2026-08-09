using System;

namespace MMORPG.Shared.DTOs;

public class CreatePartyRequest
{
    public string SessionToken { get; set; } = string.Empty;
    public Guid LeaderCharacterId { get; set; }
}
