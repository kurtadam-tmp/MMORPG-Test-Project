using System;

namespace MMORPG.Shared.DTOs;

public class GuildOperationRequest
{
    public string SessionToken { get; set; } = string.Empty;
    public Guid ActorCharacterId { get; set; }
    public Guid GuildId { get; set; }
    public Guid? TargetCharacterId { get; set; }
    public long GoldAmount { get; set; }
}
