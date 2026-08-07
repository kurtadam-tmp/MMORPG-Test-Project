namespace MMORPG.Domain.Models;

public class DungeonInstance
{
    public Guid InstanceId { get; set; } = Guid.NewGuid();
    public string DungeonTypeId { get; set; } = string.Empty;
    public Guid PartyId { get; set; }
    public int TargetZoneId { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
