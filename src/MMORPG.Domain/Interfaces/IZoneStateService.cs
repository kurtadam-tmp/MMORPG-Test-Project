namespace MMORPG.Domain.Interfaces;

public interface IZoneStateService
{
    Task<bool> RegisterPlayerInZoneAsync(int zoneId, Guid characterId, float posX, float posY, float posZ);
    Task<bool> RemovePlayerFromZoneAsync(int zoneId, Guid characterId);
    Task<long> GetZonePlayerCountAsync(int zoneId);
    Task<IEnumerable<Guid>> GetPlayersInZoneAsync(int zoneId);
}
