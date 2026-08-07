using System;
using MMORPG.Shared.Enums;

namespace MMORPG.Shared.DTOs;

public class ChatMessageDto
{
    public ChatChannel Channel { get; set; } = ChatChannel.Global;
    public Guid SenderCharacterId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string MessageText { get; set; } = string.Empty;
    public int? TargetZoneId { get; set; }
    public Guid? TargetGuildId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
