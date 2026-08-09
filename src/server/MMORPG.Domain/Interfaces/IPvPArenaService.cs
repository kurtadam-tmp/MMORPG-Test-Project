namespace MMORPG.Domain.Interfaces;

public enum ArenaMode
{
    Duel1v1 = 1,
    Team3v3 = 3,
    Battleground5v5 = 5
}

public class ArenaMatch
{
    public Guid MatchId { get; set; } = Guid.NewGuid();
    public ArenaMode Mode { get; set; } = ArenaMode.Duel1v1;
    public List<Guid> TeamRed { get; set; } = new();
    public List<Guid> TeamBlue { get; set; } = new();
    public int RedScore { get; set; }
    public int BlueScore { get; set; }
    public bool IsCompleted { get; set; }
    public Guid WinningTeamCaptain { get; set; }
}

public interface IPvPArenaService
{
    ArenaMatch QueueForArena(Guid playerCharId, ArenaMode mode);
    bool RecordKill(Guid matchId, Guid killerCharId, Guid victimCharId, out int honorAwarded);
    ArenaMatch? GetMatch(Guid matchId);
}
