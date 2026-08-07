namespace MMORPG.Domain.Interfaces;

public class PlayerHonorRecord
{
    public Guid CharacterId { get; set; }
    public string CharacterName { get; set; } = string.Empty;
    public long HonorPoints { get; set; }
    public int Kills { get; set; }
    public int Deaths { get; set; }
    public string PvPRankTitle { get; set; } = "Private";
}

public interface IHonorRankingService
{
    void AddHonor(Guid characterId, string characterName, int honorAmount);
    string GetPvPRankTitle(long honorPoints);
    IEnumerable<PlayerHonorRecord> GetTopRankings(int count);
}
