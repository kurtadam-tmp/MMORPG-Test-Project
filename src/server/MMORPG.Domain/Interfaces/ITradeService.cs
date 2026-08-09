namespace MMORPG.Domain.Interfaces;

public enum TradeStatus
{
    Pending = 0,
    Locked = 1,
    Completed = 2,
    Cancelled = 3
}

public class TradeSession
{
    public Guid SessionId { get; set; } = Guid.NewGuid();
    public Guid PlayerAId { get; set; }
    public Guid PlayerBId { get; set; }
    public long PlayerAGold { get; set; }
    public long PlayerBGold { get; set; }
    public List<Guid> PlayerAItems { get; set; } = new();
    public List<Guid> PlayerBItems { get; set; } = new();
    public bool PlayerALocked { get; set; }
    public bool PlayerBLocked { get; set; }
    public bool PlayerAConfirmed { get; set; }
    public bool PlayerBConfirmed { get; set; }
    public TradeStatus Status { get; set; } = TradeStatus.Pending;
}

public interface ITradeService
{
    TradeSession InitiateTrade(Guid senderCharId, Guid targetCharId);
    bool LockTrade(Guid sessionId, Guid playerCharId, long goldOffer, List<Guid> itemInstanceIds);
    Task<bool> ConfirmTradeAsync(Guid sessionId, Guid playerCharId);
    bool CancelTrade(Guid sessionId);
    TradeSession? GetTradeSession(Guid sessionId);
}
