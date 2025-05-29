using StackExchange.Redis;

namespace Valuator;

public class ShardRouter : IShardRouter
{
    private readonly ILogger<ShardRouter> _logger;
    private readonly Dictionary<string, string> _connectionStrings;
    private readonly Dictionary<string, ConnectionMultiplexer> _connections;

    public ShardRouter( ILogger<ShardRouter> logger, IConfiguration configuration )
    {
        _logger = logger;
        _connectionStrings = new()
        {
            [ "MAIN" ] = configuration[ "RedisConnections:MAIN" ] ?? "redis_main:6379",
            [ "RU" ] = configuration[ "RedisConnections:RU" ] ?? "redis_ru:6379",
            [ "EU" ] = configuration[ "RedisConnections:EU" ] ?? "redis_eu:6379",
            [ "ASIA" ] = configuration[ "RedisConnections:ASIA" ] ?? "redis_asia:6379",
        };

        _connections = _connectionStrings.ToDictionary(
            cs => cs.Key,
            cs => ConnectionMultiplexer.Connect( cs.Value )
        );
    }

    public IDatabase GetMainShard() => _connections[ "MAIN" ].GetDatabase();

    public IDatabase GetShardByRegion( string region )
    {
        if ( !_connections.ContainsKey( region ) )
            throw new ArgumentException( $"Unknown region: {region}" );

        return _connections[ region ].GetDatabase();
    }

    public IDatabase GetShardByKey( string key )
    {
        var mainDb = GetMainShard();
        var region = mainDb.StringGet( key );
        if ( region.IsNullOrEmpty )
            throw new Exception( $"No region found for key: {key}" );

        return GetShardByRegion( region );
    }
}

