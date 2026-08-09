using StackExchange.Redis;

namespace MMORPG.Infrastructure.Cache;

public interface IRedisConnectionFactory
{
    IConnectionMultiplexer GetConnection();
    IDatabase GetDatabase();
}

public class RedisConnectionFactory : IRedisConnectionFactory
{
    private readonly Lazy<IConnectionMultiplexer> _lazyConnection;

    public RedisConnectionFactory(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentNullException(nameof(connectionString));

        _lazyConnection = new Lazy<IConnectionMultiplexer>(() =>
        {
            var options = ConfigurationOptions.Parse(connectionString);
            options.AbortOnConnectFail = false; // Prevents crash if Redis is temporarily unreachable during startup
            return ConnectionMultiplexer.Connect(options);
        });
    }

    public IConnectionMultiplexer GetConnection() => _lazyConnection.Value;

    public IDatabase GetDatabase() => GetConnection().GetDatabase();
}
