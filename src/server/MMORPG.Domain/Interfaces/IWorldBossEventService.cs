namespace MMORPG.Domain.Interfaces;

public class WorldBossEventSession
{
    public string EventId { get; set; } = Guid.NewGuid().ToString("N");
    public string BossName { get; set; } = string.Empty;
    public int ZoneId { get; set; }
    public string LocationName { get; set; } = string.Empty;
    public int CurrentHp { get; set; } = 1000000;
    public int MaxHp { get; set; } = 1000000;
    public bool IsActive { get; set; }
    public Dictionary<Guid, int> PlayerDamageContribution { get; set; } = new();
}

public interface IWorldBossEventService
{
    WorldBossEventSession TriggerWorldBossSpawn(string bossName, int zoneId, string locationName);
    void RecordDamageContribution(string eventId, Guid playerCharId, int damageDealt);
    List<KeyValuePair<Guid, int>> ConcludeWorldBossEvent(string eventId, out string victoryAnnouncement);
    List<WorldBossEventSession> GetActiveEvents();
}
