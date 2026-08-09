using System.Collections.Concurrent;
using MMORPG.Domain.Interfaces;

namespace MMORPG.Infrastructure.Services;

public class DungeonInstancingService : IDungeonInstancingService
{
    private readonly ConcurrentDictionary<Guid, DungeonInstanceSession> _instances = new();

    public DungeonInstanceSession CreateDungeonInstance(string dungeonName, Guid partyId, List<Guid> partyMembers)
    {
        var session = new DungeonInstanceSession
        {
            InstanceId = Guid.NewGuid(),
            DungeonName = dungeonName,
            PartyGroupId = partyId,
            MemberCharacterIds = partyMembers,
            CreatedTime = DateTime.UtcNow,
            IsBossDefeated = false,
            CompletionTime = TimeSpan.Zero
        };

        _instances[session.InstanceId] = session;
        Console.WriteLine($"[DUNGEON INSTANCING] Isolated Dungeon Instance '{dungeonName}' ({session.InstanceId}) spawned for Party '{partyId}' ({partyMembers.Count} members)!");
        return session;
    }

    public bool CompleteDungeonInstance(Guid instanceId, out TimeSpan finalTime)
    {
        finalTime = TimeSpan.Zero;
        if (_instances.TryGetValue(instanceId, out var session))
        {
            lock (session)
            {
                if (session.IsBossDefeated) return false;

                session.IsBossDefeated = true;
                session.CompletionTime = DateTime.UtcNow - session.CreatedTime;
                finalTime = session.CompletionTime;
                Console.WriteLine($"[DUNGEON INSTANCING CLEARED!] Party '{session.PartyGroupId}' cleared '{session.DungeonName}' in {finalTime.TotalSeconds:F1} seconds!");
                return true;
            }
        }
        return false;
    }

    public DungeonInstanceSession GetInstanceDetails(Guid instanceId)
    {
        return _instances.TryGetValue(instanceId, out var session) ? session : null!;
    }
}
