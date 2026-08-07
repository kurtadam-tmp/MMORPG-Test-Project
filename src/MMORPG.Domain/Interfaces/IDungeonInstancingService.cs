namespace MMORPG.Domain.Interfaces;

public class DungeonInstanceSession
{
    public Guid InstanceId { get; set; } = Guid.NewGuid();
    public string DungeonName { get; set; } = string.Empty;
    public Guid PartyGroupId { get; set; }
    public List<Guid> MemberCharacterIds { get; set; } = new();
    public DateTime CreatedTime { get; set; } = DateTime.UtcNow;
    public bool IsBossDefeated { get; set; }
    public TimeSpan CompletionTime { get; set; }
}

public interface IDungeonInstancingService
{
    DungeonInstanceSession CreateDungeonInstance(string dungeonName, Guid partyId, List<Guid> partyMembers);
    bool CompleteDungeonInstance(Guid instanceId, out TimeSpan finalTime);
    DungeonInstanceSession GetInstanceDetails(Guid instanceId);
}
