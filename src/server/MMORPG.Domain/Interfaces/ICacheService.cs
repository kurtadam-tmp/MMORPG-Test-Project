namespace MMORPG.Domain.Interfaces;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key);
    Task<bool> SetAsync<T>(string key, T value, TimeSpan? timeToLive = null);
    Task<bool> RemoveAsync(string key);
    Task<bool> KeyExistsAsync(string key);
}
