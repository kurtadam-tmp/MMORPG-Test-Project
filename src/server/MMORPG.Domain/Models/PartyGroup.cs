namespace MMORPG.Domain.Models;

public class PartyGroup
{
    public Guid PartyId { get; set; } = Guid.NewGuid();
    public Guid LeaderCharacterId { get; set; }
    public List<PartyMember> Members { get; set; } = new();
    public Guid? DungeonInstanceId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
