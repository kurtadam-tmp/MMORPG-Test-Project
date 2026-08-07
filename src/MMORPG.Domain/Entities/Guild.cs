namespace MMORPG.Domain.Entities;

public class Guild
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid LeaderCharacterId { get; set; }
    public long VaultGold { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
