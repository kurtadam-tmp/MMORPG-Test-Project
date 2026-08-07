using System.Numerics;
using Microsoft.Extensions.DependencyInjection;
using MMORPG.Domain.Interfaces;
using MMORPG.Infrastructure.Cache;
using MMORPG.Infrastructure.Data;
using MMORPG.Infrastructure.Services;

Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("=================================================================");
Console.WriteLine("   MMORPG Dedicated Server - Anti-Cheat Security Live Test       ");
Console.WriteLine("=================================================================");
Console.ResetColor();

// Setup In-Memory Test Service Container
var services = new ServiceCollection();
services.AddSingleton<IDbConnectionFactory>(_ => new DbConnectionFactory("Host=localhost;Port=5432;Database=mmorpg_test;Username=postgres;Password=postgres"));
services.AddSingleton<IRedisConnectionFactory>(_ => new RedisConnectionFactory("localhost:6379"));
services.AddSingleton<ICacheService, MockCacheService>();
services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
services.AddSingleton<IPlayerSessionService, PlayerSessionService>();
services.AddSingleton<IAntiCheatValidationService, AntiCheatValidationService>();

var provider = services.BuildServiceProvider();
var antiCheat = provider.GetRequiredService<IAntiCheatValidationService>();

var charId = Guid.NewGuid();
Vector3 startPos = new Vector3(0, 0, 0);

Console.WriteLine("\n[1] Testing Normal Valid Movement (Speed: 5.0 m/s)...");
Vector3 normalPos = new Vector3(5, 0, 0);
bool normalValid = antiCheat.ValidateMovement(charId, startPos, normalPos, deltaTime: 1.0f);

if (normalValid)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("[1 PASSED] Normal movement validated cleanly.");
    Console.ResetColor();
}

Console.WriteLine("\n[2] Simulating Speed-Hack Attempt (Speed: 45.0 m/s > Max 16.5 m/s)...");
Vector3 speedHackPos = new Vector3(50, 0, 0);
bool speedHackValid = antiCheat.ValidateMovement(charId, startPos, speedHackPos, deltaTime: 1.0f);

if (!speedHackValid)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"[2 PASSED] Speed-Hack Detected & Blocked! Current Violation Count: {antiCheat.GetViolationCount(charId)}/3");
    Console.ResetColor();
}

Console.WriteLine("\n[3] Simulating Teleport-Hack Attempt (Discontinuity Distance: 120.0 m)...");
Vector3 teleportHackPos = new Vector3(120, 0, 0);
bool teleportValid = antiCheat.ValidateMovement(charId, startPos, teleportHackPos, deltaTime: 1.0f, isTeleportSpell: false);

if (!teleportValid)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"[3 PASSED] Teleport-Hack Detected & Blocked! Current Violation Count: {antiCheat.GetViolationCount(charId)}/3");
    Console.ResetColor();
}

Console.WriteLine("\n[4] Simulating Skill Cooldown Bypass Attempt (Skill #1 Required Cooldown: 3.0s)...");
antiCheat.ValidateSkillCooldown(charId, skillId: 1, requiredCooldownSeconds: 3.0f); // First cast valid
bool cooldownBypassValid = antiCheat.ValidateSkillCooldown(charId, skillId: 1, requiredCooldownSeconds: 3.0f); // Rapid re-cast attempt!

if (!cooldownBypassValid)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"[4 PASSED] Cooldown Bypass Detected & Blocked! Violation Threshold Reached: {antiCheat.GetViolationCount(charId)}/3");
    Console.ResetColor();
}

Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("\n=================================================================");
Console.WriteLine("    Anti-Cheat Security Validation Completed Successfully       ");
Console.WriteLine("=================================================================");
Console.ResetColor();

// Run Full High-Load Server Benchmark & Stress Test Suite
await MMORPG.ClientSim.StressTestRunner.RunFullServerLoadTestAsync();

// Run Castle Siege & Instanced Dungeon Live Simulation
await MMORPG.ClientSim.SiegeAndDungeonTestRunner.RunSiegeAndDungeonSimulationsAsync();


public class MockCacheService : ICacheService
{
    private readonly System.Collections.Concurrent.ConcurrentDictionary<string, string> _store = new();
    public Task<T?> GetAsync<T>(string key) => Task.FromResult(_store.TryGetValue(key, out var json) ? System.Text.Json.JsonSerializer.Deserialize<T>(json) : default);
    public Task<bool> SetAsync<T>(string key, T value, TimeSpan? timeToLive = null) { _store[key] = System.Text.Json.JsonSerializer.Serialize(value); return Task.FromResult(true); }
    public Task<bool> RemoveAsync(string key) => Task.FromResult(_store.TryRemove(key, out _));
    public Task<bool> KeyExistsAsync(string key) => Task.FromResult(_store.ContainsKey(key));
}
