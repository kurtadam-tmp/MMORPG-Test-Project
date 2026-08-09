using System;

namespace MMORPG.Domain.Models;

public class ZoneServerNode
{
    public string ServerId { get; set; } = string.Empty;
    public string IpAddress { get; set; } = "127.0.0.1";
    public int Port { get; set; } = 7777;
    public List<int> HostedZoneIds { get; set; } = new();
    public int CurrentPlayerCount { get; set; }
    public int MaxPlayerCount { get; set; } = 500;
    public string Status { get; set; } = "HEALTHY"; // HEALTHY, BUSY, OFFLINE
    public DateTime LastHeartbeatAt { get; set; } = DateTime.UtcNow;
}
