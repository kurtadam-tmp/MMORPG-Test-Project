using System.Collections.Concurrent;
using MMORPG.Domain.Interfaces;

namespace MMORPG.Infrastructure.Services;

public class PvPArenaService : IPvPArenaService
{
    private readonly IHonorRankingService _honorRankingService;
    private readonly ConcurrentDictionary<Guid, ArenaMatch> _activeMatches = new();
    private readonly ConcurrentQueue<Guid> _queue1v1 = new();

    public PvPArenaService(IHonorRankingService honorRankingService)
    {
        _honorRankingService = honorRankingService;
    }

    public ArenaMatch QueueForArena(Guid playerCharId, ArenaMode mode)
    {
        if (mode == ArenaMode.Duel1v1)
        {
            if (_queue1v1.TryDequeue(out var opponentId) && opponentId != playerCharId)
            {
                var match = new ArenaMatch
                {
                    Mode = mode,
                    TeamRed = new() { playerCharId },
                    TeamBlue = new() { opponentId }
                };
                _activeMatches[match.MatchId] = match;
                Console.WriteLine($"[PvPArena] 1v1 Arena Match '{match.MatchId}' started between '{playerCharId}' and '{opponentId}'!");
                return match;
            }

            _queue1v1.Enqueue(playerCharId);
            Console.WriteLine($"[PvPArena] Character '{playerCharId}' joined 1v1 Arena Queue.");
        }

        return new ArenaMatch { Mode = mode, TeamRed = new() { playerCharId } };
    }

    public bool RecordKill(Guid matchId, Guid killerCharId, Guid victimCharId, out int honorAwarded)
    {
        honorAwarded = 150; // 150 Honor per PvP Arena kill
        if (_activeMatches.TryGetValue(matchId, out var match))
        {
            if (match.TeamRed.Contains(killerCharId)) match.RedScore++;
            else match.BlueScore++;

            _honorRankingService.AddHonor(killerCharId, $"Gladiator_{killerCharId.ToString()[..4]}", honorAwarded);

            if (match.RedScore >= 3 || match.BlueScore >= 3)
            {
                match.IsCompleted = true;
                match.WinningTeamCaptain = match.RedScore >= 3 ? match.TeamRed[0] : match.TeamBlue[0];
                Console.WriteLine($"[PvPArena] Match '{matchId}' COMPLETED! Winner Captain: '{match.WinningTeamCaptain}'.");
            }
            return true;
        }

        // World PvP Kill
        _honorRankingService.AddHonor(killerCharId, $"Gladiator_{killerCharId.ToString()[..4]}", honorAwarded);
        return true;
    }

    public ArenaMatch? GetMatch(Guid matchId)
    {
        _activeMatches.TryGetValue(matchId, out var match);
        return match;
    }
}
