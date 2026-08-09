using System.Collections.Concurrent;
using MMORPG.Domain.Interfaces;

namespace MMORPG.Infrastructure.Services;

public class ThreatEngineService : IThreatEngineService
{
    private readonly ConcurrentDictionary<string, MobThreatTable> _tables = new();

    public void RecordDamageThreat(string mobId, Guid playerCharId, int damageAmount, bool isTaunt)
    {
        var table = _tables.GetOrAdd(mobId, id => new MobThreatTable { MobId = id });

        lock (table)
        {
            float addedThreat = damageAmount * 1.0f;
            if (isTaunt)
            {
                float maxExistingThreat = table.PlayerThreatScores.Values.DefaultIfEmpty(0f).Max();
                addedThreat = (maxExistingThreat * 1.5f) + 1000f;
                Console.WriteLine($"[ThreatEngine TAUNT] Character '{playerCharId}' TAUNTED Mob '{mobId}'! Generated +{addedThreat:F0} Threat.");
            }

            if (!table.PlayerThreatScores.ContainsKey(playerCharId))
                table.PlayerThreatScores[playerCharId] = 0f;

            table.PlayerThreatScores[playerCharId] += addedThreat;
            table.CurrentTargetPlayerId = GetHighestThreatPlayer(mobId);
        }
    }

    public void RecordHealingThreat(string mobId, Guid healerCharId, int healingAmount)
    {
        var table = _tables.GetOrAdd(mobId, id => new MobThreatTable { MobId = id });

        lock (table)
        {
            float addedThreat = healingAmount * 0.5f;
            if (!table.PlayerThreatScores.ContainsKey(healerCharId))
                table.PlayerThreatScores[healerCharId] = 0f;

            table.PlayerThreatScores[healerCharId] += addedThreat;
            table.CurrentTargetPlayerId = GetHighestThreatPlayer(mobId);
        }
    }

    public Guid GetHighestThreatPlayer(string mobId)
    {
        if (_tables.TryGetValue(mobId, out var table))
        {
            lock (table)
            {
                if (table.PlayerThreatScores.Count > 0)
                {
                    return table.PlayerThreatScores.OrderByDescending(p => p.Value).First().Key;
                }
            }
        }
        return Guid.Empty;
    }

    public MobThreatTable UpdateBossPhase(string mobId, int currentHp, int maxHp)
    {
        var table = _tables.GetOrAdd(mobId, id => new MobThreatTable { MobId = id });

        lock (table)
        {
            float hpPercent = (float)currentHp / maxHp;
            if (hpPercent <= 0.50f && table.CurrentBossPhase == 1)
            {
                table.CurrentBossPhase = 2;
                table.IsEnraged = true;
                Console.WriteLine($"[ThreatEngine BOSS ENRAGE] World Boss '{mobId}' HP dropped below 50%! PHASE 2 ENRAGE ACTIVATED (+50% Attack Speed & Fire Nova AoE)!");
            }
        }

        return table;
    }
}
