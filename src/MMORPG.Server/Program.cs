using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MMORPG.Domain.Interfaces;
using MMORPG.Infrastructure.Cache;
using MMORPG.Infrastructure.Data;
using MMORPG.Infrastructure.Network;
using MMORPG.Infrastructure.Repositories;
using MMORPG.Infrastructure.Services;
using MMORPG.Server.Engine;

// Parse Command-line Arguments (--port 7777 --zoneId 1)
int listenPort = 7777;
int zoneId = 1;

for (int i = 0; i < args.Length; i++)
{
    if (args[i] == "--port" && i + 1 < args.Length) int.TryParse(args[i + 1], out listenPort);
    if (args[i] == "--zoneId" && i + 1 < args.Length) int.TryParse(args[i + 1], out zoneId);
}

Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("=================================================");
Console.WriteLine($"   MMORPG Dedicated Server (Zone #{zoneId}, Port {listenPort}) ");
Console.WriteLine("=================================================");
Console.ResetColor();

// 1. Build Configuration
var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

var postgresConnectionString = configuration.GetConnectionString("PostgreSQL") 
    ?? "Host=localhost;Port=5432;Database=mmorpg_db;Username=postgres;Password=postgres";

var redisConnectionString = configuration.GetConnectionString("Redis")
    ?? "localhost:6379";

// 2. Setup Dependency Injection
var services = new ServiceCollection();

services.AddSingleton<IDbConnectionFactory>(_ => new DbConnectionFactory(postgresConnectionString));
services.AddSingleton<IRedisConnectionFactory>(_ => new RedisConnectionFactory(redisConnectionString));

services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
services.AddSingleton<ICacheService, RedisCacheService>();
services.AddSingleton<IPlayerSessionService, PlayerSessionService>();
services.AddSingleton<IZoneStateService, ZoneStateService>();
services.AddSingleton<IWriteBehindService, WriteBehindService>();

services.AddScoped<IAuthService, AuthService>();
services.AddScoped<ICharacterService, CharacterService>();
services.AddScoped<IMovementValidationService, MovementValidationService>();
services.AddScoped<IGatewayHandshakeService, GatewayHandshakeService>();
services.AddScoped<IInventoryService, InventoryService>();
services.AddScoped<ICombatEngineService, CombatEngineService>();
services.AddScoped<IAuctionHouseService, AuctionHouseService>();
services.AddScoped<IGuildService, GuildService>();
services.AddScoped<IQuestEngineService, QuestEngineService>();
services.AddSingleton<IDungeonPartyService, DungeonPartyService>();
services.AddSingleton<IMessageBrokerService, MessageBrokerService>();
services.AddSingleton<IMobEngineService, MobEngineService>();

services.AddScoped<INetworkPacketProcessor, NetworkPacketProcessor>();
services.AddSingleton<UdpServerListener>(_ => new UdpServerListener(
    _.GetRequiredService<INetworkPacketProcessor>(), listenPort: listenPort));

services.AddScoped<IPlayerRepository, PlayerRepository>();
services.AddScoped<ICharacterRepository, CharacterRepository>();
services.AddScoped<IStatRepository, StatRepository>();
services.AddScoped<IInventoryRepository, InventoryRepository>();
services.AddScoped<IAuctionRepository, AuctionRepository>();
services.AddScoped<IGuildRepository, GuildRepository>();
services.AddScoped<IQuestRepository, QuestRepository>();

services.AddSingleton<IGameLoop>(_ => new GameLoop(targetTickRate: 30));

var serviceProvider = services.BuildServiceProvider();

// Initialize Mobs for this Zone
var mobEngine = serviceProvider.GetRequiredService<IMobEngineService>();
mobEngine.InitializeZoneMobs(zoneId: zoneId, mobCount: zoneId == 99 ? 5 : 10);

Console.WriteLine($"[System] Registered Services: GameLoop (30 Hz), MobEngineService, UdpServerListener (Port {listenPort}).");

// 3. Initialize Game Loop
var gameLoop = serviceProvider.GetRequiredService<IGameLoop>();
gameLoop.OnTick += async (tickCount, deltaTime) =>
{
    await mobEngine.ProcessZoneMobAiTickAsync(zoneId: zoneId, deltaTime: deltaTime);

    if (tickCount % 300 == 0) // Heartbeat every 10 sec
    {
        Console.WriteLine($"[Zone #{zoneId} Tick #{tickCount}] Heartbeat - Port: {listenPort}, DeltaTime: {deltaTime * 1000:F2} ms");
    }
};

using var cts = new CancellationTokenSource();

if (Environment.GetEnvironmentVariable("RUN_ONCE") == "true")
{
    cts.CancelAfter(5000);
}

var udpListener = serviceProvider.GetRequiredService<UdpServerListener>();
_ = udpListener.StartAsync(cts.Token);

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine($"[Zone Server #{zoneId}] 30 Hz Game Loop & UDP Server Listener Active on Port {listenPort}.");
Console.ResetColor();

try
{
    await gameLoop.StartAsync(cts.Token);
}
catch (OperationCanceledException)
{
    Console.WriteLine("[System] Server shutdown signal received. Exiting cleanly.");
}
