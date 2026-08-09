using System;

namespace MMORPG.Domain.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(Guid playerId, string username, string role = "Player");
}
