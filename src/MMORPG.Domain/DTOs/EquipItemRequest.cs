namespace MMORPG.Domain.DTOs;

public class EquipItemRequest
{
    public string SessionToken { get; set; } = string.Empty;
    public Guid CharacterId { get; set; }
    public Guid ItemInstanceId { get; set; }
    public string EquipSlot { get; set; } = string.Empty; // e.g. "MAIN_HAND", "CHEST", "HEAD"
}
