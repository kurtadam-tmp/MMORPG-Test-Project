using System;

namespace MMORPG.Shared.DTOs;

public class PartyOperationRequest
{
    public string SessionToken { get; set; } = string.Empty;
    public Guid ActorCharacterId { get; set; }
    public Guid PartyId { get; set; }
    public Guid? TargetCharacterId { get; set; }
}
