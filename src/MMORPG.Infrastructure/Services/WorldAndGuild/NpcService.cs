using MMORPG.Domain.Interfaces;

namespace MMORPG.Infrastructure.Services;

public class NpcService : INpcService
{
    private static readonly List<NpcDefinition> NpcDatabase = new()
    {
        new NpcDefinition
        {
            NpcId = "npc_blacksmith_grom",
            Name = "Blacksmith Grom",
            Title = "Master Weaponsmith",
            Dialogue = "Greetings, adventurer! Need your blade sharpened or armor reinforced?",
            ShopItems = new() { "item_sword_01", "item_shield_01" }
        },
        new NpcDefinition
        {
            NpcId = "npc_stablemaster_barnaby",
            Name = "Stable Master Barnaby",
            Title = "Mount Trainer",
            Dialogue = "Looking for a swift steed to traverse the lands? I have the finest mounts in the realm!",
            ShopItems = new() { "mount_warhorse", "mount_armored_bear", "mount_inferno_drake" }
        }
    };

    public IEnumerable<NpcDefinition> GetNpcsInZone(int zoneId)
    {
        return NpcDatabase;
    }

    public NpcDefinition? GetNpcById(string npcId)
    {
        return NpcDatabase.FirstOrDefault(n => n.NpcId.Equals(npcId, StringComparison.OrdinalIgnoreCase));
    }
}
