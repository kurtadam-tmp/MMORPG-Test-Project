using MMORPG.Domain.DTOs;
using MMORPG.Domain.Interfaces;
using MMORPG.Domain.Models;

namespace MMORPG.Infrastructure.Services;

public class CombatEngineService : ICombatEngineService
{
    private readonly IPlayerSessionService _sessionService;
    private readonly ICharacterRepository _characterRepository;
    private readonly IStatRepository _statRepository;
    private readonly ICacheService _cacheService;
    private readonly IWriteBehindService _writeBehindService;

    private static readonly Dictionary<string, SkillDefinition> SkillDatabase = new()
    {
        ["slash"] = new SkillDefinition { SkillId = "slash", Name = "Slash", BaseDamage = 35, CastTimeMs = 0, CooldownMs = 1500, Range = 3.0f, ManaCost = 10, ScalingStat = "STR" },
        ["fireball"] = new SkillDefinition { SkillId = "fireball", Name = "Fireball", BaseDamage = 60, CastTimeMs = 1000, CooldownMs = 3000, Range = 15.0f, ManaCost = 25, ScalingStat = "INT" },
        ["stab"] = new SkillDefinition { SkillId = "stab", Name = "Stab", BaseDamage = 40, CastTimeMs = 0, CooldownMs = 1200, Range = 2.5f, ManaCost = 12, ScalingStat = "DEX" },
        ["smite"] = new SkillDefinition { SkillId = "smite", Name = "Smite", BaseDamage = 45, CastTimeMs = 500, CooldownMs = 2500, Range = 10.0f, ManaCost = 20, ScalingStat = "INT" }
    };

    public CombatEngineService(
        IPlayerSessionService sessionService,
        ICharacterRepository characterRepository,
        IStatRepository statRepository,
        ICacheService cacheService,
        IWriteBehindService writeBehindService)
    {
        _sessionService = sessionService;
        _characterRepository = characterRepository;
        _statRepository = statRepository;
        _cacheService = cacheService;
        _writeBehindService = writeBehindService;
    }

    private static string GetCooldownKey(Guid characterId, string skillId) => $"cooldown:{characterId}:{skillId}";

    public async Task<bool> IsSkillOnCooldownAsync(Guid characterId, string skillId)
    {
        return await _cacheService.KeyExistsAsync(GetCooldownKey(characterId, skillId));
    }

    public async Task<CombatResult> ExecuteSkillCastAsync(CastSkillRequest request)
    {
        // 1. Session Validation
        var session = await _sessionService.GetSessionAsync(request.SessionToken);
        if (session == null || session.ActiveCharacterId != request.AttackerCharacterId)
        {
            return new CombatResult { Success = false, Message = "Unauthorized combat request." };
        }

        // 2. Skill Definition Lookup
        if (!SkillDatabase.TryGetValue(request.SkillId.ToLowerInvariant(), out var skill))
        {
            return new CombatResult { Success = false, Message = "Unknown skill." };
        }

        // 3. Cooldown Check
        if (await IsSkillOnCooldownAsync(request.AttackerCharacterId, skill.SkillId))
        {
            return new CombatResult { Success = false, Message = "Skill is currently on cooldown." };
        }

        // 4. Fetch Attacker & Target Stats
        var attacker = await _characterRepository.GetByIdAsync(request.AttackerCharacterId);
        var target = await _characterRepository.GetByIdAsync(request.TargetCharacterId);

        if (attacker == null || target == null || target.IsDeleted)
        {
            return new CombatResult { Success = false, Message = "Invalid attacker or target character." };
        }

        var attackerStat = await _statRepository.GetByCharacterIdAsync(attacker.Id);
        var targetStat = await _statRepository.GetByCharacterIdAsync(target.Id);

        if (attackerStat == null || targetStat == null)
        {
            return new CombatResult { Success = false, Message = "Stats not initialized for combatants." };
        }

        // 5. Mana Cost Check
        if (attackerStat.CurrentMp < skill.ManaCost)
        {
            return new CombatResult { Success = false, Message = "Insufficient mana." };
        }

        // 6. Range Check (3D Euclidean Distance)
        float dx = request.TargetX - attacker.PosX;
        float dy = request.TargetY - attacker.PosY;
        float dz = request.TargetZ - attacker.PosZ;
        float distance = MathF.Sqrt(dx * dx + dy * dy + dz * dz);

        if (distance > skill.Range + 1.0f) // 1 unit tolerance
        {
            return new CombatResult { Success = false, Message = "Target out of range." };
        }

        // 7. Calculate Dodge Chance (Target Agility * 0.3%)
        float dodgeChance = targetStat.Agility * 0.003f;
        if (Random.Shared.NextSingle() < dodgeChance)
        {
            // Apply Mana Cost & Set Cooldown
            attackerStat.CurrentMp -= skill.ManaCost;
            await _statRepository.UpdateAsync(attackerStat);
            await _cacheService.SetAsync(GetCooldownKey(attacker.Id, skill.SkillId), true, TimeSpan.FromMilliseconds(skill.CooldownMs));

            return new CombatResult
            {
                Success = true,
                IsDodged = true,
                DamageDealt = 0,
                TargetCurrentHp = targetStat.CurrentHp,
                Message = $"Attack dodged by '{target.Name}'."
            };
        }

        // 8. Calculate Base Damage
        float rawDamage = skill.ScalingStat switch
        {
            "STR" => skill.BaseDamage + (attackerStat.Strength * 1.5f) + (attackerStat.Agility * 0.5f),
            "INT" => skill.BaseDamage + (attackerStat.Intelligence * 2.2f),
            "DEX" => skill.BaseDamage + (attackerStat.Agility * 1.8f) + (attackerStat.Strength * 0.5f),
            _ => skill.BaseDamage
        };

        // 9. Calculate Critical Strike Chance (Agility * 0.5%)
        bool isCritical = Random.Shared.NextSingle() < (attackerStat.Agility * 0.005f);
        if (isCritical)
        {
            rawDamage *= 1.5f;
        }

        // 10. Armor Damage Reduction (Vitality Scaling)
        float armorReduction = targetStat.Vitality / (targetStat.Vitality + 100.0f);
        int finalDamage = Math.Max(1, (int)(rawDamage * (1.0f - armorReduction)));

        // 11. Apply Mana Cost & Health Reduction
        attackerStat.CurrentMp -= skill.ManaCost;
        targetStat.CurrentHp = Math.Max(0, targetStat.CurrentHp - finalDamage);

        bool targetIsDead = targetStat.CurrentHp <= 0;

        // 12. Save Stats & Set Redis Cooldown
        await _statRepository.UpdateAsync(attackerStat);
        await _statRepository.UpdateAsync(targetStat);
        await _cacheService.SetAsync(GetCooldownKey(attacker.Id, skill.SkillId), true, TimeSpan.FromMilliseconds(skill.CooldownMs));

        // Mark target character dirty for Write-Behind flushing
        await _writeBehindService.MarkCharacterDirtyAsync(target.Id);

        return new CombatResult
        {
            Success = true,
            DamageDealt = finalDamage,
            IsCritical = isCritical,
            IsDodged = false,
            TargetCurrentHp = targetStat.CurrentHp,
            TargetIsDead = targetIsDead,
            CooldownRemainingMs = skill.CooldownMs,
            Message = targetIsDead ? $"Target '{target.Name}' slain by '{attacker.Name}'!" : $"Dealt {finalDamage} damage to '{target.Name}'."
        };
    }
}
