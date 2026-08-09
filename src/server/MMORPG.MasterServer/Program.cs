using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MMORPG.Domain.Interfaces;
using MMORPG.Domain.Models;
using MMORPG.Infrastructure.Cache;
using MMORPG.Infrastructure.Data;
using MMORPG.Infrastructure.Services;

Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("=================================================================");
Console.WriteLine("     MMORPG Master Cluster & Multi-Zone Load Balancer Service    ");
Console.WriteLine("=================================================================");
Console.ResetColor();

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) =>
    {
        services.AddSingleton<IDbConnectionFactory>(_ => new DbConnectionFactory("Host=localhost;Port=5432;Database=mmorpg_db;Username=postgres;Password=postgres"));
        services.AddSingleton<IRedisConnectionFactory>(_ => new RedisConnectionFactory("localhost:6379"));
        services.AddSingleton<ICacheService, RedisCacheService>();
        services.AddSingleton<IMasterClusterService, MasterClusterService>();
    })
    .Build();

var clusterService = host.Services.GetRequiredService<IMasterClusterService>();

// Register initial Zone Server Nodes in Cluster
await clusterService.RegisterZoneNodeAsync(new ZoneServerNode
{
    ServerId = "zone-node-us-east-1",
    IpAddress = "127.0.0.1",
    Port = 7777,
    HostedZoneIds = new List<int> { 1, 2 },
    MaxPlayerCount = 500
});

await clusterService.RegisterZoneNodeAsync(new ZoneServerNode
{
    ServerId = "dungeon-node-us-east-1",
    IpAddress = "127.0.0.1",
    Port = 7778,
    HostedZoneIds = new List<int> { 99 },
    MaxPlayerCount = 200
});

// Run Cluster Load Balancing Query Test
var bestZone1 = await clusterService.GetBestZoneServerAsync(targetZoneId: 1);
Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine($"[Cluster Balance] Target Zone #1 routed to Node '{bestZone1?.ServerId}' ({bestZone1?.IpAddress}:{bestZone1?.Port}).");

var bestDungeon = await clusterService.GetBestZoneServerAsync(targetZoneId: 99);
Console.WriteLine($"[Cluster Balance] Target Dungeon Zone #99 routed to Node '{bestDungeon?.ServerId}' ({bestDungeon?.IpAddress}:{bestDungeon?.Port}).");
Console.ResetColor();

Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("=================================================================");
Console.WriteLine("     Master Cluster Service Initialized & Monitoring 2 Nodes    ");
Console.WriteLine("=================================================================");
Console.ResetColor();

await host.RunAsync();
