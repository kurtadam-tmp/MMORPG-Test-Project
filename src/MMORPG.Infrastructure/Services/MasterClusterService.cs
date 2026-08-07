using System.Collections.Concurrent;
using MMORPG.Domain.Interfaces;
using MMORPG.Domain.Models;

namespace MMORPG.Infrastructure.Services;

public class MasterClusterService : IMasterClusterService
{
    private readonly ICacheService _cacheService;
    private readonly ConcurrentDictionary<string, ZoneServerNode> _memoryNodes = new();
    private static readonly TimeSpan HeartbeatTimeout = TimeSpan.FromSeconds(15);

    public MasterClusterService(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public Task<bool> RegisterZoneNodeAsync(ZoneServerNode node)
    {
        node.LastHeartbeatAt = DateTime.UtcNow;
        node.Status = "HEALTHY";
        _memoryNodes[node.ServerId] = node;

        Console.WriteLine($"[MasterCluster] Zone Server Node '{node.ServerId}' ({node.IpAddress}:{node.Port}) registered for Zones [{string.Join(",", node.HostedZoneIds)}].");
        return Task.FromResult(true);
    }

    public Task<bool> SendHeartbeatAsync(string serverId, int currentPlayerCount)
    {
        if (_memoryNodes.TryGetValue(serverId, out var node))
        {
            node.CurrentPlayerCount = currentPlayerCount;
            node.LastHeartbeatAt = DateTime.UtcNow;
            node.Status = node.CurrentPlayerCount >= node.MaxPlayerCount ? "BUSY" : "HEALTHY";
            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }

    public Task<ZoneServerNode?> GetBestZoneServerAsync(int targetZoneId)
    {
        var now = DateTime.UtcNow;
        
        // Filter active nodes that host the target zone and haven't timed out
        var validNodes = _memoryNodes.Values
            .Where(n => n.HostedZoneIds.Contains(targetZoneId) && (now - n.LastHeartbeatAt) <= HeartbeatTimeout && n.Status != "OFFLINE")
            .OrderBy(n => n.CurrentPlayerCount) // Load Balancing: Select node with lowest player count
            .FirstOrDefault();

        return Task.FromResult(validNodes);
    }

    public Task<IEnumerable<ZoneServerNode>> GetAllNodesAsync()
    {
        return Task.FromResult<IEnumerable<ZoneServerNode>>(_memoryNodes.Values);
    }
}
