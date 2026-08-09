using System.Collections.Concurrent;
using MMORPG.Domain.Enums;
using MMORPG.Domain.Interfaces;
using MMORPG.Domain.Models;

namespace MMORPG.Infrastructure.Services;

public class MobEngineService : IMobEngineService
{
    private readonly IZoneStateService _zoneStateService;
    private readonly ICharacterRepository _characterRepository;
    private readonly IQuestEngineService _questEngineService;

    private readonly ConcurrentDictionary<int, ConcurrentDictionary<Guid, MobEntity>> _zoneMobs = new();

    private static readonly Dictionary<string, MobDefinition> MobDatabase = new()
    {
        ["goblin"] = new MobDefinition { MobTypeId = "goblin", Name = "Forest Goblin", MaxHp = 150, BaseDamage = 15, MoveSpeed = 4.5f, AggroRadius = 8.0f, AttackRange = 2.0f, RespawnTimeMs = 10000, ExperienceReward = 50 },
        ["wolf"] = new MobDefinition { MobTypeId = "wolf", Name = "Wild Wolf", MaxHp = 300, BaseDamage = 25, MoveSpeed = 5.5f, AggroRadius = 10.0f, AttackRange = 2.2f, RespawnTimeMs = 15000, ExperienceReward = 85 },
        ["orc"] = new MobDefinition { MobTypeId = "orc", Name = "Orc Warrior", MaxHp = 550, BaseDamage = 45, MoveSpeed = 4.0f, AggroRadius = 9.0f, AttackRange = 2.5f, RespawnTimeMs = 20000, ExperienceReward = 140 },
        ["lava_elemental"] = new MobDefinition { MobTypeId = "lava_elemental", Name = "Lava Elemental", MaxHp = 1800, BaseDamage = 120, MoveSpeed = 3.5f, AggroRadius = 12.0f, AttackRange = 3.0f, RespawnTimeMs = 30000, ExperienceReward = 450 },
        ["boss_ignis"] = new MobDefinition { MobTypeId = "boss_ignis", Name = "Inferno Dragon Ignis (Raid Boss)", MaxHp = 100000, BaseDamage = 850, MoveSpeed = 6.0f, AggroRadius = 25.0f, AttackRange = 5.0f, RespawnTimeMs = 300000, ExperienceReward = 15000 },
        ["boss_kelthuzis"] = new MobDefinition { MobTypeId = "boss_kelthuzis", Name = "Frost Lich Kel'Thuzis (Raid Boss)", MaxHp = 250000, BaseDamage = 1400, MoveSpeed = 5.0f, AggroRadius = 30.0f, AttackRange = 8.0f, RespawnTimeMs = 600000, ExperienceReward = 35000 }
    };

    public MobEngineService(
        IZoneStateService zoneStateService,
        ICharacterRepository characterRepository,
        IQuestEngineService questEngineService)
    {
        _zoneStateService = zoneStateService;
        _characterRepository = characterRepository;
        _questEngineService = questEngineService;
    }

    public void InitializeZoneMobs(int zoneId, int mobCount)
    {
        var mobs = _zoneMobs.GetOrAdd(zoneId, _ => new ConcurrentDictionary<Guid, MobEntity>());
        mobs.Clear();

        var mobTypes = MobDatabase.Keys.ToArray();

        for (int i = 0; i < mobCount; i++)
        {
            var mobTypeId = mobTypes[i % mobTypes.Length];
            var def = MobDatabase[mobTypeId];

            float spawnX = (Random.Shared.NextSingle() - 0.5f) * 50.0f;
            float spawnY = 0.0f;
            float spawnZ = (Random.Shared.NextSingle() - 0.5f) * 50.0f;

            var entity = new MobEntity
            {
                InstanceId = Guid.NewGuid(),
                MobTypeId = def.MobTypeId,
                Name = def.Name,
                ZoneId = zoneId,
                SpawnX = spawnX,
                SpawnY = spawnY,
                SpawnZ = spawnZ,
                CurrentX = spawnX,
                CurrentY = spawnY,
                CurrentZ = spawnZ,
                CurrentHp = def.MaxHp,
                MaxHp = def.MaxHp,
                State = MobState.Idle
            };

            mobs.TryAdd(entity.InstanceId, entity);
        }

        Console.WriteLine($"[MobEngine] Zone #{zoneId} initialized with {mobs.Count} mobs.");
    }

    public IEnumerable<MobEntity> GetActiveZoneMobs(int zoneId)
    {
        if (_zoneMobs.TryGetValue(zoneId, out var mobs))
        {
            return mobs.Values.Where(m => m.State != MobState.Dead);
        }
        return Enumerable.Empty<MobEntity>();
    }

    public async Task ProcessZoneMobAiTickAsync(int zoneId, float deltaTime)
    {
        if (!_zoneMobs.TryGetValue(zoneId, out var mobs) || mobs.IsEmpty) return;

        List<Guid> activePlayersInZone = new();
        try
        {
            activePlayersInZone = (await _zoneStateService.GetPlayersInZoneAsync(zoneId)).ToList();
        }
        catch
        {
            // Redis offline or connection pending fallback
        }

        foreach (var mob in mobs.Values)
        {
            if (!MobDatabase.TryGetValue(mob.MobTypeId, out var def)) continue;

            switch (mob.State)
            {
                case MobState.Dead:
                    if (DateTime.UtcNow >= mob.RespawnTime)
                    {
                        mob.CurrentHp = mob.MaxHp;
                        mob.CurrentX = mob.SpawnX;
                        mob.CurrentY = mob.SpawnY;
                        mob.CurrentZ = mob.SpawnZ;
                        mob.State = MobState.Idle;
                        mob.TargetCharacterId = null;
                        Console.WriteLine($"[MobEngine] Mob '{mob.Name}' (ID: {mob.InstanceId}) respawned in Zone #{zoneId}.");
                    }
                    break;

                case MobState.Idle:
                case MobState.Patrol:
                    // Check for nearby players within AggroRadius
                    foreach (var charId in activePlayersInZone)
                    {
                        var playerChar = await _characterRepository.GetByIdAsync(charId);
                        if (playerChar != null)
                        {
                            float dist = Distance3D(mob.CurrentX, mob.CurrentY, mob.CurrentZ, playerChar.PosX, playerChar.PosY, playerChar.PosZ);
                            if (dist <= def.AggroRadius)
                            {
                                mob.TargetCharacterId = charId;
                                mob.State = MobState.Chasing;
                                Console.WriteLine($"[MobEngine] Mob '{mob.Name}' aggroed on Player '{playerChar.Name}'!");
                                break;
                            }
                        }
                    }
                    break;

                case MobState.Chasing:
                    if (!mob.TargetCharacterId.HasValue)
                    {
                        mob.State = MobState.Idle;
                        break;
                    }

                    var targetChar = await _characterRepository.GetByIdAsync(mob.TargetCharacterId.Value);
                    if (targetChar == null)
                    {
                        mob.State = MobState.Idle;
                        mob.TargetCharacterId = null;
                        break;
                    }

                    float distToTarget = Distance3D(mob.CurrentX, mob.CurrentY, mob.CurrentZ, targetChar.PosX, targetChar.PosY, targetChar.PosZ);

                    if (distToTarget > def.AggroRadius * 2.0f) // Leash reset
                    {
                        mob.State = MobState.Idle;
                        mob.TargetCharacterId = null;
                        mob.CurrentX = mob.SpawnX;
                        mob.CurrentZ = mob.SpawnZ;
                    }
                    else if (distToTarget <= def.AttackRange)
                    {
                        mob.State = MobState.Attacking;
                    }
                    else
                    {
                        // Move towards target
                        float dx = targetChar.PosX - mob.CurrentX;
                        float dz = targetChar.PosZ - mob.CurrentZ;
                        float len = MathF.Sqrt(dx * dx + dz * dz);
                        if (len > 0.01f)
                        {
                            mob.CurrentX += (dx / len) * def.MoveSpeed * deltaTime;
                            mob.CurrentZ += (dz / len) * def.MoveSpeed * deltaTime;
                        }
                    }
                    break;

                case MobState.Attacking:
                    if (DateTime.UtcNow >= mob.NextStateChangeTime)
                    {
                        // Attack cooldown
                        mob.NextStateChangeTime = DateTime.UtcNow.AddMilliseconds(1500);
                        mob.State = MobState.Chasing;
                    }
                    break;
            }
        }
    }

    public async Task<bool> ApplyDamageToMobAsync(Guid mobInstanceId, int damage, Guid attackerCharacterId)
    {
        foreach (var zone in _zoneMobs.Values)
        {
            if (zone.TryGetValue(mobInstanceId, out var mob))
            {
                if (mob.State == MobState.Dead) return false;

                mob.CurrentHp = Math.Max(0, mob.CurrentHp - damage);
                mob.TargetCharacterId = attackerCharacterId;
                mob.State = MobState.Chasing;

                if (mob.CurrentHp <= 0)
                {
                    mob.State = MobState.Dead;
                    if (MobDatabase.TryGetValue(mob.MobTypeId, out var def))
                    {
                        mob.RespawnTime = DateTime.UtcNow.AddMilliseconds(def.RespawnTimeMs);

                        // Award XP to attacker
                        var attacker = await _characterRepository.GetByIdAsync(attackerCharacterId);
                        if (attacker != null)
                        {
                            await _characterRepository.UpdateExperienceAndLevelAsync(
                                attacker.Id, 
                                attacker.Experience + def.ExperienceReward, 
                                attacker.Level);

                            // Record Quest Kill Progress
                            await _questEngineService.RecordMobKillAsync(attacker.Id, mob.MobTypeId);

                            Console.WriteLine($"[MobEngine] Mob '{mob.Name}' slain by '{attacker.Name}'! Awarded {def.ExperienceReward} XP.");
                        }
                    }
                }

                return true;
            }
        }
        return false;
    }

    private static float Distance3D(float x1, float y1, float z1, float x2, float y2, float z2)
    {
        float dx = x2 - x1;
        float dy = y2 - y1;
        float dz = z2 - z1;
        return MathF.Sqrt(dx * dx + dy * dy + dz * dz);
    }
}
