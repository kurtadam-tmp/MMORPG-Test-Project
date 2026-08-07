using MMORPG.Shared.Enums;

namespace MMORPG.Domain.Models;

public class PartyMember
{
    public Guid CharacterId { get; set; }
    public string CharacterName { get; set; } = string.Empty;
    public int Level { get; set; }
    public CharacterClass Class { get; set; }
    public int CurrentHp { get; set; }
    public int MaxHp { get; set; }
    public bool IsLeader { get; set; }
}
