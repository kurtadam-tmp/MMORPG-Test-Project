using System;
using System.Numerics;

namespace MMORPG.Domain.Interfaces;

public interface IAntiCheatValidationService
{
    bool ValidateMovement(Guid characterId, Vector3 oldPos, Vector3 newPos, float deltaTime, bool isTeleportSpell = false);
    bool ValidateSkillCooldown(Guid characterId, int skillId, float requiredCooldownSeconds);
    bool ValidatePacketRate(Guid characterId, int maxPacketsPerSecond = 60);
    Task<bool> RecordViolationAsync(Guid characterId, string violationType, string details);
    int GetViolationCount(Guid characterId);
}
