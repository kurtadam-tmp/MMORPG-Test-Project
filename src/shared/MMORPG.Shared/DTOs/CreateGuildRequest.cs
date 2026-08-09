using System;

namespace MMORPG.Shared.DTOs;

public class CreateGuildRequest
{
    public string SessionToken { get; set; } = string.Empty;
    public Guid LeaderCharacterId { get; set; }
    public string GuildName { get; set; } = string.Empty;
}
