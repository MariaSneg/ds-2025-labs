using RabbitMQ.Client;
using StackExchange.Redis;

namespace RankCalculator;

public class ShardManager : IShardManager
{
    private readonly ILogger<ShardManager> _logger;
    private readonly Dictionary<string, ConnectionMultiplexer> _connections;
    private readonly Dictionary<string, string> _connectionStrings = new();
    private IDatabase _currentDatabase;

    public ShardManager( ILogger<ShardManager> logger, IConfiguration configuration )
    {
        _logger = logger;

        _connectionStrings.Add( "MAIN", configuration[ "RedisConnections:MAIN" ] ?? "redis_main:6379" );
        _connectionStrings.Add( "RU", configuration[ "RedisConnections:RU" ] ?? "redis_ru:6379" );
        _connectionStrings.Add( "EU", configuration[ "RedisConnections:EU" ] ?? "redis_eu:6379" );
        _connectionStrings.Add( "ASIA", configuration[ "RedisConnections:ASIA" ] ?? "redis_asia:6379" );

        _connections = _connectionStrings.ToDictionary(
            cs => cs.Key,
            cs => ConnectionMultiplexer.Connect( cs.Value )
        );
    }

    public void SetRegionShard( string key )
    {
        var mainDb = _connections[ "MAIN" ].GetDatabase();
        var region = mainDb.StringGet( key ).ToString();

        _logger.LogInformation( $"LOOKUP {key}, {region}" );

        if ( _connections.ContainsKey( region ) )
        {
            _currentDatabase = _connections[ region ].GetDatabase();
        }
        else
        {
            throw new InvalidOperationException( $"Unknown region '{region}' for key '{key}'" );
        }
    }

    public void SetRank( string key, double value )
    {
        _currentDatabase?.StringSet( "RANK-" + key, value );
    }

    public RedisValue Get( string key, string prefix )
    {
        return _currentDatabase?.StringGet( prefix + key ) ?? RedisValue.Null;
    }
}
