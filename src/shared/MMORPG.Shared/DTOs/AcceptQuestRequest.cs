using System;

namespace MMORPG.Shared.DTOs;

public class AcceptQuestRequest
{
    public string SessionToken { get; set; } = string.Empty;
    public Guid CharacterId { get; set; }
    public string QuestId { get; set; } = string.Empty;
}
