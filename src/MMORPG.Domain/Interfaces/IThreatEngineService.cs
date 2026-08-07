namespace MMORPG.Domain.Interfaces;

public class MobThreatTable
{
    public string MobId { get; set; } = string.Empty;
    public Guid CurrentTargetPlayerId { get; set; }
    public Dictionary<Guid, float> PlayerThreatScores { get; set; } = new();
    public int CurrentBossPhase { get; set; } = 1;
    public bool IsEnraged { get; set; }
}

public interface IThreatEngineService
{
    void RecordDamageThreat(string mobId, Guid playerCharId, int damageAmount, bool isTaunt);
    void RecordHealingThreat(string mobId, Guid healerCharId, int healingAmount);
    Guid GetHighestThreatPlayer(string mobId);
    MobThreatTable UpdateBossPhase(string mobId, int currentHp, int maxHp);
}
