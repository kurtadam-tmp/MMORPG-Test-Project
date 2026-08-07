namespace MMORPG.Domain.Entities;

public class Stat
{
    public Guid CharacterId { get; set; }
    public int Strength { get; set; } = 10;
    public int Agility { get; set; } = 10;
    public int Intelligence { get; set; } = 10;
    public int Vitality { get; set; } = 10;
    public int CurrentHp { get; set; } = 100;
    public int MaxHp { get; set; } = 100;
    public int CurrentMp { get; set; } = 50;
    public int MaxMp { get; set; } = 50;
    public int UnallocatedPoints { get; set; } = 0;
    public long Gold { get; set; } = 100;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
