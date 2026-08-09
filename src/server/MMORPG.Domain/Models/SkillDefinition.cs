namespace MMORPG.Domain.Models;

public class SkillDefinition
{
    public string SkillId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int BaseDamage { get; set; }
    public int CastTimeMs { get; set; }
    public int CooldownMs { get; set; }
    public float Range { get; set; }
    public int ManaCost { get; set; }
    public string ScalingStat { get; set; } = "STR"; // STR, INT, DEX
}
