namespace MMORPG.Domain.Models;

public class MobDefinition
{
    public string MobTypeId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int MaxHp { get; set; }
    public int BaseDamage { get; set; }
    public float MoveSpeed { get; set; }
    public float AggroRadius { get; set; }
    public float AttackRange { get; set; }
    public int RespawnTimeMs { get; set; }
    public long ExperienceReward { get; set; }
}
