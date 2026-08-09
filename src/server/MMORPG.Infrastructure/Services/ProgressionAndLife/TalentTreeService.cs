using System.Collections.Concurrent;
using MMORPG.Domain.Interfaces;

namespace MMORPG.Infrastructure.Services;

public class TalentTreeService : ITalentTreeService
{
    private readonly ConcurrentDictionary<Guid, CharacterTalentTree> _trees = new();

    private static readonly Dictionary<string, List<TalentNode>> ClassSpecTemplates = new()
    {
        {
            "Warrior", new()
            {
                new TalentNode { TalentId = "warrior_arms_crit", Name = "Cruelty", SpecName = "Arms", Tier = 1, MaxPoints = 5, Description = "+1% Critical Hit Chance per point." },
                new TalentNode { TalentId = "warrior_fury_flurry", Name = "Flurry", SpecName = "Fury", Tier = 2, MaxPoints = 5, Description = "+6% Attack Speed after a critical hit." },
                new TalentNode { TalentId = "warrior_prot_shieldwall", Name = "Shield Wall", SpecName = "Protection", Tier = 3, MaxPoints = 1, Description = "Reduces all damage taken by 60% for 10 seconds." }
            }
        },
        {
            "Mage", new()
            {
                new TalentNode { TalentId = "mage_fire_ignite", Name = "Ignite", SpecName = "Fire", Tier = 1, MaxPoints = 5, Description = "Critical fire strikes burn target for +8% damage over 4 sec." },
                new TalentNode { TalentId = "mage_frost_iceblock", Name = "Ice Block", SpecName = "Frost", Tier = 2, MaxPoints = 1, Description = "Encases you in ice, making you immune to all damage for 10 sec." },
                new TalentNode { TalentId = "mage_arcane_power", Name = "Arcane Power", SpecName = "Arcane", Tier = 3, MaxPoints = 1, Description = "Increases spell damage by +30% for 15 seconds." }
            }
        }
    };

    public CharacterTalentTree GetTalentTreeForCharacter(Guid characterId, string characterClass)
    {
        return _trees.GetOrAdd(characterId, id =>
        {
            var template = ClassSpecTemplates.TryGetValue(characterClass, out var nodes) ? nodes : new();
            return new CharacterTalentTree
            {
                CharacterId = id,
                ActiveSpec = template.FirstOrDefault()?.SpecName ?? "General",
                UnallocatedTalentPoints = 15, // 15 Talent points available at level 25
                AllocatedTalents = template.Select(t => new TalentNode
                {
                    TalentId = t.TalentId,
                    Name = t.Name,
                    SpecName = t.SpecName,
                    Tier = t.Tier,
                    MaxPoints = t.MaxPoints,
                    CurrentPoints = 0,
                    Description = t.Description
                }).ToList()
            };
        });
    }

    public bool AllocateTalentPoint(Guid characterId, string talentId, out int newPoints)
    {
        newPoints = 0;
        if (_trees.TryGetValue(characterId, out var tree))
        {
            if (tree.UnallocatedTalentPoints <= 0) return false;

            var talent = tree.AllocatedTalents.FirstOrDefault(t => t.TalentId.Equals(talentId, StringComparison.OrdinalIgnoreCase));
            if (talent != null && talent.CurrentPoints < talent.MaxPoints)
            {
                talent.CurrentPoints++;
                tree.UnallocatedTalentPoints--;
                newPoints = talent.CurrentPoints;
                Console.WriteLine($"[TalentService SUCCESS] Character '{characterId}' allocated point into '{talent.Name}' ({talent.CurrentPoints}/{talent.MaxPoints})! Points Remaining: {tree.UnallocatedTalentPoints}.");
                return true;
            }
        }

        return false;
    }

    public bool ResetTalents(Guid characterId, out int refundedPoints)
    {
        refundedPoints = 0;
        if (_trees.TryGetValue(characterId, out var tree))
        {
            int pointsToRefund = tree.AllocatedTalents.Sum(t => t.CurrentPoints);
            foreach (var talent in tree.AllocatedTalents)
            {
                talent.CurrentPoints = 0;
            }
            tree.UnallocatedTalentPoints += pointsToRefund;
            refundedPoints = tree.UnallocatedTalentPoints;
            Console.WriteLine($"[TalentService RESPEC] Character '{characterId}' reset all talents! Refunded {pointsToRefund} points (Total Unallocated: {refundedPoints}).");
            return true;
        }

        return false;
    }
}
