namespace MMORPG.Domain.Interfaces;

public class TalentNode
{
    public string TalentId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string SpecName { get; set; } = string.Empty;
    public int Tier { get; set; } = 1;
    public int MaxPoints { get; set; } = 5;
    public int CurrentPoints { get; set; } = 0;
    public string Description { get; set; } = string.Empty;
}

public class CharacterTalentTree
{
    public Guid CharacterId { get; set; }
    public string ActiveSpec { get; set; } = "Primary";
    public int UnallocatedTalentPoints { get; set; }
    public List<TalentNode> AllocatedTalents { get; set; } = new();
}

public interface ITalentTreeService
{
    CharacterTalentTree GetTalentTreeForCharacter(Guid characterId, string characterClass);
    bool AllocateTalentPoint(Guid characterId, string talentId, out int newPoints);
    bool ResetTalents(Guid characterId, out int refundedPoints);
}
