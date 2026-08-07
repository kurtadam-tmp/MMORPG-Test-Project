namespace MMORPG.Domain.Interfaces;

public class CastleState
{
    public string CastleId { get; set; } = string.Empty;
    public string CastleName { get; set; } = "Ironforge Fortress";
    public Guid OwningGuildId { get; set; }
    public string OwningGuildName { get; set; } = "Unclaimed Neutral Castle";
    public int GateHealth { get; set; } = 100000;
    public int RelicCrystalHealth { get; set; } = 50000;
    public bool IsSiegeActive { get; set; }
    public long AccumulatedTaxGold { get; set; }
}

public interface ICastleSiegeService
{
    CastleState GetCastleState(string castleId);
    bool StartSiegeWar(string castleId);
    bool AttackCastleGate(string castleId, Guid attackerGuildId, int damage, out int remainingGateHp);
    bool CaptureRelicCrystal(string castleId, Guid capturerGuildId, string capturerGuildName);
    long ClaimGuildTaxGold(string castleId, Guid guildLeaderCharId);
}
