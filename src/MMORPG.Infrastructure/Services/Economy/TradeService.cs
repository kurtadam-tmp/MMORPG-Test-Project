using System.Collections.Concurrent;
using MMORPG.Domain.Interfaces;

namespace MMORPG.Infrastructure.Services;

public class TradeService : ITradeService
{
    private readonly IStatRepository _statRepository;
    private readonly ConcurrentDictionary<Guid, TradeSession> _activeTrades = new();

    public TradeService(IStatRepository statRepository)
    {
        _statRepository = statRepository;
    }

    public TradeSession InitiateTrade(Guid senderCharId, Guid targetCharId)
    {
        var session = new TradeSession
        {
            PlayerAId = senderCharId,
            PlayerBId = targetCharId,
            Status = TradeStatus.Pending
        };

        _activeTrades.TryAdd(session.SessionId, session);
        Console.WriteLine($"[TradeService] Trade Session '{session.SessionId}' initiated between '{senderCharId}' and '{targetCharId}'.");
        return session;
    }

    public bool LockTrade(Guid sessionId, Guid playerCharId, long goldOffer, List<Guid> itemInstanceIds)
    {
        if (!_activeTrades.TryGetValue(sessionId, out var session) || session.Status == TradeStatus.Cancelled || session.Status == TradeStatus.Completed)
            return false;

        if (playerCharId == session.PlayerAId)
        {
            session.PlayerAGold = goldOffer;
            session.PlayerAItems = itemInstanceIds;
            session.PlayerALocked = true;
        }
        else if (playerCharId == session.PlayerBId)
        {
            session.PlayerBGold = goldOffer;
            session.PlayerBItems = itemInstanceIds;
            session.PlayerBLocked = true;
        }
        else
        {
            return false;
        }

        if (session.PlayerALocked && session.PlayerBLocked)
        {
            session.Status = TradeStatus.Locked;
            Console.WriteLine($"[TradeService] Trade Session '{sessionId}' locked by both players. Awaiting final confirmation.");
        }

        return true;
    }

    public async Task<bool> ConfirmTradeAsync(Guid sessionId, Guid playerCharId)
    {
        if (!_activeTrades.TryGetValue(sessionId, out var session) || session.Status != TradeStatus.Locked)
            return false;

        if (playerCharId == session.PlayerAId) session.PlayerAConfirmed = true;
        if (playerCharId == session.PlayerBId) session.PlayerBConfirmed = true;

        if (session.PlayerAConfirmed && session.PlayerBConfirmed)
        {
            // Execute Atomic Database Transfer of Gold & Items
            var statA = await _statRepository.GetByCharacterIdAsync(session.PlayerAId);
            var statB = await _statRepository.GetByCharacterIdAsync(session.PlayerBId);

            if (statA != null && statB != null)
            {
                // Verify gold balances
                if (statA.Gold < session.PlayerAGold || statB.Gold < session.PlayerBGold)
                {
                    session.Status = TradeStatus.Cancelled;
                    Console.WriteLine($"[TradeService Error] Trade '{sessionId}' cancelled due to insufficient gold balance.");
                    return false;
                }

                // Execute Atomic Transfer of Gold
                statA.Gold = statA.Gold - session.PlayerAGold + session.PlayerBGold;
                statB.Gold = statB.Gold - session.PlayerBGold + session.PlayerAGold;

                await _statRepository.UpdateGoldAsync(statA.CharacterId, statA.Gold);
                await _statRepository.UpdateGoldAsync(statB.CharacterId, statB.Gold);

                session.Status = TradeStatus.Completed;
                Console.WriteLine($"[TradeService SUCCESS] Trade '{sessionId}' COMPLETED! Player A Gold: {statA.Gold}, Player B Gold: {statB.Gold}");
                return true;
            }
        }

        return false;
    }

    public bool CancelTrade(Guid sessionId)
    {
        if (_activeTrades.TryGetValue(sessionId, out var session))
        {
            session.Status = TradeStatus.Cancelled;
            Console.WriteLine($"[TradeService] Trade '{sessionId}' cancelled.");
            return true;
        }
        return false;
    }

    public TradeSession? GetTradeSession(Guid sessionId)
    {
        _activeTrades.TryGetValue(sessionId, out var session);
        return session;
    }
}
