using MMORPG.Domain.DTOs;
using MMORPG.Domain.Interfaces;

namespace MMORPG.Infrastructure.Services;

public class MovementValidationService : IMovementValidationService
{
    private readonly IPlayerSessionService _sessionService;
    private readonly ICharacterRepository _characterRepository;
    private readonly IZoneStateService _zoneStateService;
    private readonly IWriteBehindService _writeBehindService;

    // Movement validation parameters
    private const float BaseMoveSpeed = 6.0f; // 6.0 world units per second
    private const float ToleranceMultiplier = 1.30f; // 30% tolerance buffer for latency jitter
    private const float MinimumDeltaTimeSeconds = 0.016f; // Minimum 16ms delta

    public MovementValidationService(
        IPlayerSessionService sessionService,
        ICharacterRepository characterRepository,
        IZoneStateService zoneStateService,
        IWriteBehindService writeBehindService)
    {
        _sessionService = sessionService;
        _characterRepository = characterRepository;
        _zoneStateService = zoneStateService;
        _writeBehindService = writeBehindService;
    }

    public async Task<MovementValidationResult> ValidateAndApplyMovementAsync(MovementInputRequest request)
    {
        // 1. Validate Session & Active Character Ownership
        var session = await _sessionService.GetSessionAsync(request.SessionToken);
        if (session == null || session.ActiveCharacterId != request.CharacterId)
        {
            return new MovementValidationResult
            {
                IsValid = false,
                SequenceId = request.SequenceId,
                Message = "Unauthorized movement input for active character."
            };
        }

        // 2. Fetch Last Known Position from DB or Redis
        var character = await _characterRepository.GetByIdAsync(request.CharacterId);
        if (character == null)
        {
            return new MovementValidationResult
            {
                IsValid = false,
                SequenceId = request.SequenceId,
                Message = "Character not found."
            };
        }

        float lastX = character.PosX;
        float lastY = character.PosY;
        float lastZ = character.PosZ;

        // 3. Compute 3D Euclidean Distance
        float dx = request.TargetX - lastX;
        float dy = request.TargetY - lastY;
        float dz = request.TargetZ - lastZ;
        float distanceMoved = MathF.Sqrt(dx * dx + dy * dy + dz * dz);

        // 4. Calculate Maximum Allowed Movement Distance
        // Assuming tick/input delta time approx 33ms (0.033s)
        float estimatedDeltaTime = 0.033f;
        float maxAllowedDistance = BaseMoveSpeed * estimatedDeltaTime * ToleranceMultiplier;

        // 5. Speed hack / Teleport hack Sanity Check
        if (distanceMoved > maxAllowedDistance)
        {
            Console.WriteLine($"[Anti-Cheat Warning] Speedhack/Teleport detected for Char '{character.Name}'. Distance: {distanceMoved:F2}, MaxAllowed: {maxAllowedDistance:F2}");

            // Trigger Rubberband Rollback to last valid position
            return new MovementValidationResult
            {
                IsValid = false,
                IsRubberbandTriggered = true,
                CorrectedX = lastX,
                CorrectedY = lastY,
                CorrectedZ = lastZ,
                SequenceId = request.SequenceId,
                Message = "Movement validation failed. Rubberband rollback triggered."
            };
        }

        // 6. Valid Movement: Update Redis Zone State immediately (Fast Cache)
        await _zoneStateService.RegisterPlayerInZoneAsync(
            character.ZoneId, 
            character.Id, 
            request.TargetX, 
            request.TargetY, 
            request.TargetZ);

        // 7. Mark character dirty for Write-Behind PostgreSQL persistence
        await _writeBehindService.MarkCharacterDirtyAsync(character.Id);

        return new MovementValidationResult
        {
            IsValid = true,
            IsRubberbandTriggered = false,
            CorrectedX = request.TargetX,
            CorrectedY = request.TargetY,
            CorrectedZ = request.TargetZ,
            SequenceId = request.SequenceId,
            Message = "Movement validated and cached successfully."
        };
    }
}
