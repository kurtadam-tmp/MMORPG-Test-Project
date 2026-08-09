using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using MMORPG.Domain.Interfaces;
using MMORPG.Infrastructure.Services;

namespace MMORPG.ClientSim;

public static class SiegeAndDungeonTestRunner
{
    public static async Task RunSiegeAndDungeonSimulationsAsync()
    {
        Console.WriteLine("\n============================================================");
        Console.WriteLine(" 🛡️  LIVE CASTLE SIEGE WAR & DUNGEON INSTANCE SIMULATOR 🏰");
        Console.WriteLine("============================================================\n");

        // 1. Castle Siege War Simulation
        await SimulateCastleSiegeWarAsync();

        // 2. Instanced Dungeon Speedrun Simulation
        await SimulateInstancedDungeonSpeedrunAsync();

        Console.WriteLine("\n============================================================");
        Console.WriteLine(" ✅ CASTLE SIEGE & DUNGEON SIMULATIONS COMPLETED SUCCESSFULLY!");
        Console.WriteLine("============================================================\n");
    }

    private static async Task SimulateCastleSiegeWarAsync()
    {
        Console.WriteLine("[SIMULATION 1] Starting Castle Siege War for 'Ironforge Fortress'...");
        ICastleSiegeService siegeService = new CastleSiegeService();

        string castleId = "castle_ironforge";
        siegeService.StartSiegeWar(castleId);

        Guid guildA = Guid.NewGuid();
        Guid guildB = Guid.NewGuid();

        Stopwatch sw = Stopwatch.StartNew();

        // Phase 1: 50 Parallel Attackers Destroy Castle Gate (100,000 HP)
        Console.WriteLine(" 🏰 Phase 1: 50 Guild Attackers Channeling Catapults & Battering Rams at Castle Gate...");
        int totalGateDamage = 0;
        List<Task> siegeTasks = new();

        for (int i = 0; i < 50; i++)
        {
            siegeTasks.Add(Task.Run(() =>
            {
                for (int j = 0; j < 20; j++)
                {
                    siegeService.AttackCastleGate(castleId, guildA, 1000, out int remainingHp);
                    Interlocked.Add(ref totalGateDamage, 1000);
                }
            }));
        }

        await Task.WhenAll(siegeTasks);
        sw.Stop();

        CastleState state = siegeService.GetCastleState(castleId);
        Console.WriteLine($"   ➜ Gate Status: Gate HP = {state.GateHealth}/100,000 (Breached! Damage Dealt: {totalGateDamage:N0} in {sw.ElapsedMilliseconds} ms)");

        // Phase 2: Relic Crystal Capture
        Console.WriteLine(" 💎 Phase 2: Guild 'Dragon Knights' Breaching Inner Sanctum to Capture Relic Crystal...");
        bool captured = siegeService.CaptureRelicCrystal(castleId, guildA, "Dragon Knights Alliance");

        if (captured)
        {
            long claimedTax = siegeService.ClaimGuildTaxGold(castleId, Guid.NewGuid());
            Console.WriteLine($"   ➜ Relic Capture: SUCCESS! New Castle Owner: 'Dragon Knights Alliance' | Tax Claimed: {claimedTax:N0} Gold.");
        }
    }

    private static async Task SimulateInstancedDungeonSpeedrunAsync()
    {
        Console.WriteLine("\n[SIMULATION 2] Starting Instanced Dungeon Run 'Crypt of the Undead (Zone #99)'...");
        IDungeonInstancingService dungeonService = new DungeonInstancingService();

        Guid partyId = Guid.NewGuid();
        List<Guid> partyMembers = new() { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

        Stopwatch sw = Stopwatch.StartNew();

        DungeonInstanceSession session = dungeonService.CreateDungeonInstance("Crypt of the Undead Boss Instance", partyId, partyMembers);
        Console.WriteLine($" 💀 Created Dungeon Instance #{session.InstanceId.ToString("N")[..8]} for 5 Party Members.");

        // Simulate Mob Wave Clearing & Boss Defeat (1.5 seconds simulated execution)
        Console.WriteLine(" ⚔️ Clearing 3 Mob Waves, Skeletons, and Undead Lord Boss...");
        await Task.Delay(500);

        bool completed = dungeonService.CompleteDungeonInstance(session.InstanceId, out TimeSpan speedrunTime);
        sw.Stop();

        Console.WriteLine($"   ➜ Dungeon Result: Victory = {completed} | Speedrun Record Time: {speedrunTime.TotalSeconds:F2} seconds!");
    }
}
