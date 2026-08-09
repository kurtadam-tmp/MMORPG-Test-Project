namespace MMORPG.Domain.Interfaces;

public class ResourceNodeDefinition
{
    public string NodeId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ProfessionRequired { get; set; } = "Mining";
    public int RequiredSkillLevel { get; set; } = 1;
    public string HarvestedItemTemplateId { get; set; } = string.Empty;
    public int HarvestQuantity { get; set; } = 1;
    public int RespawnSeconds { get; set; } = 30;
}

public interface IGatheringService
{
    IEnumerable<ResourceNodeDefinition> GetNodesInZone(int zoneId);
    bool HarvestNode(Guid characterId, string nodeId, out string harvestedItem, out int quantity, out int expGained);
}
