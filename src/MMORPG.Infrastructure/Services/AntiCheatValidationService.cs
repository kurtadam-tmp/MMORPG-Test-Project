using System.Collections.Concurrent;
using System.Numerics;
using MMORPG.Domain.Interfaces;

namespace MMORPG.Infrastructure.Services;

public class AntiCheatValidationService : IAntiCheatValidationService
{
    private readonly IPlayerSessionService _sessionService;
    
    // Anti-Cheat Constants
    private const float MaxAllowedSpeedMetersPerSec = 16.5f; // 15 m/s base + 10% network jitter tolerance
    private const float MaxTeleportDiscontinuityMeters = 35.0f;
    private const int MaxViolationsBeforeKick = 3;

    // In-memory violation and rate tracking
    private readonly ConcurrentDictionary<Guid, int> _violationCounts = new();
    private readonly ConcurrentDictionary<(Guid CharId, int SkillId), DateTime> _skillLastCastTime = new();
    private readonly ConcurrentDictionary<Guid, (int PacketCount, DateTime WindowStart)> _packetRateTracker = new();

    public AntiCheatValidationService(IPlayerSessionService sessionService)
    {
        _sessionService = sessionService;
    }

    public bool ValidateMovement(Guid characterId, Vector3 oldPos, Vector3 newPos, float deltaTime, bool isTeleportSpell = false)
    {
        if (deltaTime <= 0.001f) return true; // Ignore micro frame updates

        float distance = Vector3.Distance(oldPos, newPos);
        float calculatedSpeed = distance / deltaTime;

        // 1. Speed Hack Validation
        if (calculatedSpeed > MaxAllowedSpeedMetersPerSec && !isTeleportSpell)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[AntiCheat ALERT] Speed-hack detected for Char '{characterId}'! Speed: {calculatedSpeed:F2} m/s (Max: {MaxAllowedSpeedMetersPerSec:F2} m/s)");
            Console.ResetColor();
            
            _ = RecordViolationAsync(characterId, "SPEED_HACK", $"Speed {calculatedSpeed:F2} m/s exceeded max allowed.");
            return false;
        }

        // 2. Teleport Hack Discontinuity Validation
        if (distance > MaxTeleportDiscontinuityMeters && !isTeleportSpell)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[AntiCheat ALERT] Teleport-hack detected for Char '{characterId}'! Distance: {distance:F2} m (Max: {MaxTeleportDiscontinuityMeters:F2} m)");
            Console.ResetColor();

            _ = RecordViolationAsync(characterId, "TELEPORT_HACK", $"Discontinuity distance {distance:F2} m without spell flag.");
            return false;
        }

        return true;
    }

    public bool ValidateSkillCooldown(Guid characterId, int skillId, float requiredCooldownSeconds)
    {
        var key = (characterId, skillId);
        var now = DateTime.UtcNow;

        if (_skillLastCastTime.TryGetValue(key, out var lastCast))
        {
            double elapsedSeconds = (now - lastCast).TotalSeconds;
            if (elapsedSeconds < (requiredCooldownSeconds - 0.15f)) // 150ms network latency tolerance
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[AntiCheat ALERT] Skill Cooldown bypass attempt for Char '{characterId}', Skill #{skillId}! Elapsed: {elapsedSeconds:F2}s, Required: {requiredCooldownSeconds:F2}s");
                Console.ResetColor();

                _ = RecordViolationAsync(characterId, "COOLDOWN_BYPASS", $"Skill {skillId} cast too fast ({elapsedSeconds:F2}s vs {requiredCooldownSeconds:F2}s).");
                return false;
            }
        }

        _skillLastCastTime[key] = now;
        return true;
    }

    public bool ValidatePacketRate(Guid characterId, int maxPacketsPerSecond = 60)
    {
        var now = DateTime.UtcNow;
        _packetRateTracker.AddOrUpdate(characterId,
            _ => (1, now),
            (_, current) =>
            {
                if ((now - current.WindowStart).TotalSeconds >= 1.0)
                {
                    return (1, now);
                }
                return (current.PacketCount + 1, current.WindowStart);
            });

        var (count, _) = _packetRateTracker[characterId];
        if (count > maxPacketsPerSecond)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[AntiCheat ALERT] Packet flooding rate exceeded for Char '{characterId}'! Rate: {count} pkts/sec (Max: {maxPacketsPerSecond})");
            Console.ResetColor();

            _ = RecordViolationAsync(characterId, "PACKET_FLOOD", $"Packet rate {count} pkts/sec exceeded max {maxPacketsPerSecond}.");
            return false;
        }

        return true;
    }

    public async Task<bool> RecordViolationAsync(Guid characterId, string violationType, string details)
    {
        int newCount = _violationCounts.AddOrUpdate(characterId, 1, (_, current) => current + 1);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"[AntiCheat VIOLATION #{newCount}/{MaxViolationsBeforeKick}] Character '{characterId}' -> Type: {violationType} ({details})");
        Console.ResetColor();

        if (newCount >= MaxViolationsBeforeKick)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine($"[AntiCheat ACTION] KICK & BAN THRESHOLD REACHED for Character '{characterId}'! Terminating Session & Revoking Access Token...");
            Console.ResetColor();

            // Invalidate player session
            await _sessionService.RevokeSessionAsync(characterId.ToString());
            return true; // Indicates kick was triggered
        }

        return false;
    }

    public int GetViolationCount(Guid characterId)
    {
        return _violationCounts.TryGetValue(characterId, out int count) ? count : 0;
    }
}
