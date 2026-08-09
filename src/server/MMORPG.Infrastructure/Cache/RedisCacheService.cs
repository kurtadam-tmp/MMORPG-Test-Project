using System.Text.Json;
using MMORPG.Domain.Interfaces;
using StackExchange.Redis;

namespace MMORPG.Infrastructure.Cache;

public class RedisCacheService : ICacheService
{
    private readonly IRedisConnectionFactory _connectionFactory;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public RedisCacheService(IRedisConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var db = _connectionFactory.GetDatabase();
        var value = await db.StringGetAsync(key);
        if (value.IsNull) return default;

        return JsonSerializer.Deserialize<T>(value.ToString(), _jsonOptions);
    }

    public async Task<bool> SetAsync<T>(string key, T value, TimeSpan? timeToLive = null)
    {
        var db = _connectionFactory.GetDatabase();
        var json = JsonSerializer.Serialize(value, _jsonOptions);
        if (timeToLive.HasValue)
        {
            return await db.StringSetAsync(key, json, timeToLive.Value);
        }
        return await db.StringSetAsync(key, json);
    }

    public async Task<bool> RemoveAsync(string key)
    {
        var db = _connectionFactory.GetDatabase();
        return await db.KeyDeleteAsync(key);
    }

    public async Task<bool> KeyExistsAsync(string key)
    {
        var db = _connectionFactory.GetDatabase();
        return await db.KeyExistsAsync(key);
    }
}
