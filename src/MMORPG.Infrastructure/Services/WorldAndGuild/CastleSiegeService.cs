using System.Collections.Concurrent;
using MMORPG.Domain.Interfaces;

namespace MMORPG.Infrastructure.Services;

public class CastleSiegeService : ICastleSiegeService
{
    private readonly ConcurrentDictionary<string, CastleState> _castles = new();

    public CastleSiegeService()
    {
        _castles["castle_ironforge"] = new CastleState
        {
            CastleId = "castle_ironforge",
            CastleName = "Ironforge Fortress",
            OwningGuildId = Guid.Empty,
            OwningGuildName = "Unclaimed Neutral Castle",
            GateHealth = 100000,
            RelicCrystalHealth = 50000,
            IsSiegeActive = false,
            AccumulatedTaxGold = 25000
        };
    }

    public CastleState GetCastleState(string castleId)
    {
        return _castles.TryGetValue(castleId, out var castle) ? castle : null!;
    }

    public bool StartSiegeWar(string castleId)
    {
        if (_castles.TryGetValue(castleId, out var castle))
        {
            lock (castle)
            {
                castle.IsSiegeActive = true;
                castle.GateHealth = 100000;
                castle.RelicCrystalHealth = 50000;
                Console.WriteLine($"[CASTLE SIEGE WAR STARTED!] The Weekly Siege War for '{castle.CastleName}' HAS BEGUN!");
                return true;
            }
        }
        return false;
    }

    public bool AttackCastleGate(string castleId, Guid attackerGuildId, int damage, out int remainingGateHp)
    {
        remainingGateHp = 0;
        if (_castles.TryGetValue(castleId, out var castle) && castle.IsSiegeActive)
        {
            lock (castle)
            {
                castle.GateHealth = Math.Max(0, castle.GateHealth - damage);
                remainingGateHp = castle.GateHealth;
                if (remainingGateHp == 0)
                {
                    Console.WriteLine($"[CASTLE SIEGE GATE BREACHED!] Castle Gate of '{castle.CastleName}' HAS BEEN DESTROYED!");
                }
                return true;
            }
        }
        return false;
    }

    public bool CaptureRelicCrystal(string castleId, Guid capturerGuildId, string capturerGuildName)
    {
        if (_castles.TryGetValue(castleId, out var castle) && castle.IsSiegeActive)
        {
            lock (castle)
            {
                if (castle.GateHealth > 0) return false; // Gate must be breached first

                castle.OwningGuildId = capturerGuildId;
                castle.OwningGuildName = capturerGuildName;
                castle.IsSiegeActive = false;
                Console.WriteLine($"[CASTLE SIEGE VICTORY!] Guild '{capturerGuildName}' HAS CAPTURED '{castle.CastleName}'!");
                return true;
            }
        }
        return false;
    }

    public long ClaimGuildTaxGold(string castleId, Guid guildLeaderCharId)
    {
        if (_castles.TryGetValue(castleId, out var castle))
        {
            lock (castle)
            {
                long taxToClaim = castle.AccumulatedTaxGold;
                castle.AccumulatedTaxGold = 0;
                Console.WriteLine($"[CASTLE TAX CLAIMED] Guild Leader '{guildLeaderCharId}' claimed {taxToClaim} Tax Gold from '{castle.CastleName}'.");
                return taxToClaim;
            }
        }
        return 0;
    }
}
