using System.Collections.Concurrent;
using MMORPG.Domain.Interfaces;

namespace MMORPG.Infrastructure.Services;

public class HonorRankingService : IHonorRankingService
{
    private readonly ConcurrentDictionary<Guid, PlayerHonorRecord> _rankings = new();

    public void AddHonor(Guid characterId, string characterName, int honorAmount)
    {
        var record = _rankings.GetOrAdd(characterId, id => new PlayerHonorRecord
        {
            CharacterId = id,
            CharacterName = characterName,
            HonorPoints = 0,
            Kills = 0,
            Deaths = 0,
            PvPRankTitle = "Private"
        });

        record.HonorPoints += honorAmount;
        record.Kills++;
        record.PvPRankTitle = GetPvPRankTitle(record.HonorPoints);

        Console.WriteLine($"[HonorRanking] Character '{characterName}' gained +{honorAmount} Honor (Total: {record.HonorPoints}, Rank: {record.PvPRankTitle})!");
    }

    public string GetPvPRankTitle(long honorPoints)
    {
        if (honorPoints >= 15000) return "Grand Marshal / High Warlord";
        if (honorPoints >= 5000)  return "Commander / Champion";
        if (honorPoints >= 1000)  return "Knight / Stone Guard";
        return "Private / Scout";
    }

    public IEnumerable<PlayerHonorRecord> GetTopRankings(int count)
    {
        return _rankings.Values.OrderByDescending(r => r.HonorPoints).Take(count);
    }
}
