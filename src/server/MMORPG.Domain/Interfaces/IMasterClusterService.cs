using MMORPG.Domain.Models;

namespace MMORPG.Domain.Interfaces;

public interface IMasterClusterService
{
    Task<bool> RegisterZoneNodeAsync(ZoneServerNode node);
    Task<bool> SendHeartbeatAsync(string serverId, int currentPlayerCount);
    Task<ZoneServerNode?> GetBestZoneServerAsync(int targetZoneId);
    Task<IEnumerable<ZoneServerNode>> GetAllNodesAsync();
}
