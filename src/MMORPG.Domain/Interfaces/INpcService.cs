namespace MMORPG.Domain.Interfaces;

public class NpcDefinition
{
    public string NpcId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Dialogue { get; set; } = string.Empty;
    public List<string> ShopItems { get; set; } = new();
}

public interface INpcService
{
    IEnumerable<NpcDefinition> GetNpcsInZone(int zoneId);
    NpcDefinition? GetNpcById(string npcId);
}
