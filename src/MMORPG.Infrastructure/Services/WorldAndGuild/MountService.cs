using System.Collections.Concurrent;
using MMORPG.Domain.Interfaces;

namespace MMORPG.Infrastructure.Services;

public class MountService : IMountService
{
    private static readonly List<MountDefinition> Mounts = new()
    {
        new MountDefinition { MountId = "mount_warhorse", Name = "Armored Warhorse", SpeedMultiplier = 1.6f, RequiredLevel = 20, Rarity = "Rare" },
        new MountDefinition { MountId = "mount_armored_bear", Name = "Battle Bear", SpeedMultiplier = 1.8f, RequiredLevel = 40, Rarity = "Epic" },
        new MountDefinition { MountId = "mount_inferno_drake", Name = "Inferno Drake (Flying)", SpeedMultiplier = 2.8f, RequiredLevel = 60, Rarity = "Legendary" }
    };

    private readonly ConcurrentDictionary<Guid, string> _mountedPlayers = new();

    public IEnumerable<MountDefinition> GetAvailableMounts() => Mounts;

    public bool SummonMount(Guid characterId, string mountId, out float newMovementSpeed)
    {
        var mount = Mounts.FirstOrDefault(m => m.MountId.Equals(mountId, StringComparison.OrdinalIgnoreCase));
        if (mount == null)
        {
            newMovementSpeed = 5.0f;
            return false;
        }

        _mountedPlayers[characterId] = mountId;
        newMovementSpeed = 5.0f * mount.SpeedMultiplier;
        Console.WriteLine($"[MountService] Character '{characterId}' mounted '{mount.Name}'! Speed boosted to {newMovementSpeed:F1} m/s.");
        return true;
    }

    public bool Dismount(Guid characterId, out float baseMovementSpeed)
    {
        baseMovementSpeed = 5.0f;
        if (_mountedPlayers.TryRemove(characterId, out _))
        {
            Console.WriteLine($"[MountService] Character '{characterId}' dismounted. Speed restored to {baseMovementSpeed:F1} m/s.");
            return true;
        }
        return false;
    }
}
