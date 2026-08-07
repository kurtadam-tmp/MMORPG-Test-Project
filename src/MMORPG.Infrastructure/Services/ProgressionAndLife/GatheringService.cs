using MMORPG.Domain.Interfaces;

namespace MMORPG.Infrastructure.Services;

public class GatheringService : IGatheringService
{
    private static readonly List<ResourceNodeDefinition> ResourceNodes = new()
    {
        new ResourceNodeDefinition { NodeId = "node_iron_01", Name = "Rich Iron Vein", ProfessionRequired = "Mining", RequiredSkillLevel = 1, HarvestedItemTemplateId = "item_iron_ore", HarvestQuantity = 3, RespawnSeconds = 45 },
        new ResourceNodeDefinition { NodeId = "node_peacebloom_01", Name = "Wild Peacebloom", ProfessionRequired = "Herbalism", RequiredSkillLevel = 1, HarvestedItemTemplateId = "item_herb_peacebloom", HarvestQuantity = 2, RespawnSeconds = 30 },
        new ResourceNodeDefinition { NodeId = "node_oak_01", Name = "Ancient Oak Tree", ProfessionRequired = "Woodcutting", RequiredSkillLevel = 1, HarvestedItemTemplateId = "item_wood", HarvestQuantity = 4, RespawnSeconds = 40 },
        new ResourceNodeDefinition { NodeId = "node_mithril_01", Name = "Glowing Mithril Deposit", ProfessionRequired = "Mining", RequiredSkillLevel = 35, HarvestedItemTemplateId = "item_ore_mithril", HarvestQuantity = 2, RespawnSeconds = 90 }
    };

    public IEnumerable<ResourceNodeDefinition> GetNodesInZone(int zoneId) => ResourceNodes;

    public bool HarvestNode(Guid characterId, string nodeId, out string harvestedItem, out int quantity, out int expGained)
    {
        var node = ResourceNodes.FirstOrDefault(n => n.NodeId.Equals(nodeId, StringComparison.OrdinalIgnoreCase));
        if (node == null)
        {
            harvestedItem = string.Empty;
            quantity = 0;
            expGained = 0;
            return false;
        }

        harvestedItem = node.HarvestedItemTemplateId;
        quantity = node.HarvestQuantity;
        expGained = 25 * node.RequiredSkillLevel;

        Console.WriteLine($"[Gathering HARVEST] Character '{characterId}' harvested '{quantity}x {node.HarvestedItemTemplateId}' from '{node.Name}' (+{expGained} {node.ProfessionRequired} EXP)!");
        return true;
    }
}
