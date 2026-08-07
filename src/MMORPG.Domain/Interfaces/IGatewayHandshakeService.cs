using MMORPG.Domain.DTOs;

namespace MMORPG.Domain.Interfaces;

public interface IGatewayHandshakeService
{
    Task<ZoneHandoffToken?> IssueHandoffTokenAsync(string sessionToken, Guid characterId, int targetZoneId);
    Task<ZoneHandshakeResult> ValidateAndConsumeHandshakeTokenAsync(ZoneHandshakeRequest request);
}
