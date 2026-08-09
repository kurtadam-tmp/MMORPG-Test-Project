using MMORPG.Domain.Enums;

namespace MMORPG.Domain.DTOs;

public class ChatMessageDto
{
    public ChatChannel Channel { get; set; }
    public Guid SenderCharacterId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public int? TargetZoneId { get; set; }
    public Guid? TargetGuildId { get; set; }
    public string MessageText { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
