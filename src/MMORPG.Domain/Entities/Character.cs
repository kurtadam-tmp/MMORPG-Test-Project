using MMORPG.Domain.Enums;

namespace MMORPG.Domain.Entities;

public class Character
{
    public Guid Id { get; set; }
    public Guid PlayerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Level { get; set; } = 1;
    public long Experience { get; set; } = 0;
    public CharacterClass CharacterClass { get; set; }
    public float PosX { get; set; } = 0.0f;
    public float PosY { get; set; } = 0.0f;
    public float PosZ { get; set; } = 0.0f;
    public int ZoneId { get; set; } = 1;
    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property (optional domain reference)
    public Stat? Stat { get; set; }
    public List<InventoryItem> InventoryItems { get; set; } = new();
}
