using MMORPG.Domain.DTOs;

namespace MMORPG.Domain.Interfaces;

public interface IMovementValidationService
{
    Task<MovementValidationResult> ValidateAndApplyMovementAsync(MovementInputRequest request);
}
