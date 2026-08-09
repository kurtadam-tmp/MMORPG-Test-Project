namespace MMORPG.Domain.Interfaces;

public class MountDefinition
{
    public string MountId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public float SpeedMultiplier { get; set; } = 1.6f;
    public int RequiredLevel { get; set; } = 20;
    public string Rarity { get; set; } = "Rare";
}

public interface IMountService
{
    IEnumerable<MountDefinition> GetAvailableMounts();
    bool SummonMount(Guid characterId, string mountId, out float newMovementSpeed);
    bool Dismount(Guid characterId, out float baseMovementSpeed);
}
