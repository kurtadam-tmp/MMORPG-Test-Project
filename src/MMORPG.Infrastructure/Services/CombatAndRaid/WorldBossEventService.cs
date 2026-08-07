using System.Collections.Concurrent;
using MMORPG.Domain.Interfaces;

namespace MMORPG.Infrastructure.Services;

public class WorldBossEventService : IWorldBossEventService
{
    private readonly ConcurrentDictionary<string, WorldBossEventSession> _events = new();

    public WorldBossEventSession TriggerWorldBossSpawn(string bossName, int zoneId, string locationName)
    {
        var session = new WorldBossEventSession
        {
            EventId = Guid.NewGuid().ToString("N"),
            BossName = bossName,
            ZoneId = zoneId,
            LocationName = locationName,
            CurrentHp = 1000000,
            MaxHp = 1000000,
            IsActive = true,
            PlayerDamageContribution = new Dictionary<Guid, int>()
        };

        _events[session.EventId] = session;
        Console.WriteLine($"[GLOBAL BROADCAST] ATTENTION HEROES! World Boss '{bossName}' HAS SPAWNED AT '{locationName}' (Zone #{zoneId})!");
        return session;
    }

    public void RecordDamageContribution(string eventId, Guid playerCharId, int damageDealt)
    {
        if (_events.TryGetValue(eventId, out var session) && session.IsActive)
        {
            lock (session)
            {
                session.CurrentHp = Math.Max(0, session.CurrentHp - damageDealt);
                if (!session.PlayerDamageContribution.ContainsKey(playerCharId))
                    session.PlayerDamageContribution[playerCharId] = 0;

                session.PlayerDamageContribution[playerCharId] += damageDealt;
                Console.WriteLine($"[WORLD BOSS RAID] Player '{playerCharId}' dealt {damageDealt} Dmg to '{session.BossName}'! Remaining HP: {session.CurrentHp}/{session.MaxHp}.");
            }
        }
    }

    public List<KeyValuePair<Guid, int>> ConcludeWorldBossEvent(string eventId, out string victoryAnnouncement)
    {
        victoryAnnouncement = string.Empty;
        if (_events.TryGetValue(eventId, out var session))
        {
            lock (session)
            {
                session.IsActive = false;
                var topContributors = session.PlayerDamageContribution.OrderByDescending(p => p.Value).Take(10).ToList();
                var topPlayer = topContributors.FirstOrDefault();

                victoryAnnouncement = $"TEBRİKLER! Dünya Bossu '{session.BossName}' YIKILDI! En Yüksek Hasar Veren Şampiyon: '{topPlayer.Key}' ({topPlayer.Value} Hasar)!";
                Console.WriteLine($"[GLOBAL BROADCAST] {victoryAnnouncement}");
                return topContributors;
            }
        }
        return new List<KeyValuePair<Guid, int>>();
    }

    public List<WorldBossEventSession> GetActiveEvents()
    {
        return _events.Values.Where(e => e.IsActive).ToList();
    }
}
