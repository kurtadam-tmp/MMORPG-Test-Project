using MMORPG.Domain.Enums;

namespace MMORPG.Domain.Models;

public class MobEntity
{
    public Guid InstanceId { get; set; } = Guid.NewGuid();
    public string MobTypeId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int ZoneId { get; set; }
    public float SpawnX { get; set; }
    public float SpawnY { get; set; }
    public float SpawnZ { get; set; }
    public float CurrentX { get; set; }
    public float CurrentY { get; set; }
    public float CurrentZ { get; set; }
    public int CurrentHp { get; set; }
    public int MaxHp { get; set; }
    public MobState State { get; set; } = MobState.Idle;
    public Guid? TargetCharacterId { get; set; }
    public DateTime NextStateChangeTime { get; set; } = DateTime.UtcNow;
    public DateTime RespawnTime { get; set; } = DateTime.UtcNow;
}
