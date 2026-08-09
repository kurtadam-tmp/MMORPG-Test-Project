using MMORPG.Domain.Enums;

namespace MMORPG.Domain.DTOs;

public class CharacterDto
{
    public Guid Id { get; set; }
    public Guid PlayerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Level { get; set; }
    public long Experience { get; set; }
    public CharacterClass CharacterClass { get; set; }
    public float PosX { get; set; }
    public float PosY { get; set; }
    public float PosZ { get; set; }
    public int ZoneId { get; set; }
    public DateTime CreatedAt { get; set; }

    // Stat properties
    public int Strength { get; set; }
    public int Agility { get; set; }
    public int Intelligence { get; set; }
    public int Vitality { get; set; }
    public int CurrentHp { get; set; }
    public int MaxHp { get; set; }
    public int CurrentMp { get; set; }
    public int MaxMp { get; set; }
    public int UnallocatedPoints { get; set; }
}
