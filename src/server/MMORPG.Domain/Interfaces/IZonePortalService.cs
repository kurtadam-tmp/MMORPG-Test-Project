namespace MMORPG.Domain.Interfaces;

public class ZonePortalNode
{
    public string PortalId { get; set; } = string.Empty;
    public int SourceZoneId { get; set; }
    public int TargetZoneId { get; set; }
    public string DestinationName { get; set; } = string.Empty;
    public int RequiredLevel { get; set; } = 1;
    public string TargetHost { get; set; } = "127.0.0.1";
    public int TargetPort { get; set; } = 7778;
}

public interface IZonePortalService
{
    bool TraversePortal(string portalId, Guid characterId, int characterLevel, out string handoffToken, out ZonePortalNode destination);
    List<ZonePortalNode> GetPortalsInZone(int zoneId);
}
