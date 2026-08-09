using System.Collections.Concurrent;
using MMORPG.Domain.Interfaces;

namespace MMORPG.Infrastructure.Services;

public class ZonePortalService : IZonePortalService
{
    private readonly ConcurrentDictionary<string, ZonePortalNode> _portals = new();

    public ZonePortalService()
    {
        _portals["portal_101"] = new ZonePortalNode
        {
            PortalId = "portal_101",
            SourceZoneId = 1,
            TargetZoneId = 2,
            DestinationName = "Frost Peaks Summit",
            RequiredLevel = 10,
            TargetHost = "127.0.0.1",
            TargetPort = 7778
        };

        _portals["portal_102"] = new ZonePortalNode
        {
            PortalId = "portal_102",
            SourceZoneId = 2,
            TargetZoneId = 99,
            DestinationName = "Crypt of the Undead Boss Instance",
            RequiredLevel = 25,
            TargetHost = "127.0.0.1",
            TargetPort = 7779
        };
    }

    public bool TraversePortal(string portalId, Guid characterId, int characterLevel, out string handoffToken, out ZonePortalNode destination)
    {
        handoffToken = string.Empty;
        destination = null!;

        if (_portals.TryGetValue(portalId, out var portal))
        {
            if (characterLevel < portal.RequiredLevel)
            {
                Console.WriteLine($"[ZONE PORTAL REJECTED] Character '{characterId}' Level {characterLevel} too low for Portal '{portalId}' (Required: Level {portal.RequiredLevel}).");
                return false;
            }

            destination = portal;
            handoffToken = Guid.NewGuid().ToString("N");
            Console.WriteLine($"[ZONE PORTAL TRAVERSE SUCCESS] Character '{characterId}' traversing Portal '{portalId}' to '{portal.DestinationName}' (Zone #{portal.TargetZoneId}, Port {portal.TargetPort})! Handoff Token: {handoffToken}.");
            return true;
        }

        return false;
    }

    public List<ZonePortalNode> GetPortalsInZone(int zoneId)
    {
        return _portals.Values.Where(p => p.SourceZoneId == zoneId).ToList();
    }
}
