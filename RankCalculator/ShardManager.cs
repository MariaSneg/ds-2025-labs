using StackExchange.Redis;
using Microsoft.Extensions.Logging;

namespace RankCalculator;

public class ShardManager : IShardManager
{
    private readonly Dictionary<string, string> _shardConectionStringDictionary;
    private readonly Dictionary<string, ConnectionMultiplexer> _shardConnections;
    private readonly ILogger<ShardManager> _logger;
    private IDatabase _database;

    public ShardManager( ILogger<ShardManager> logger )
    {
        _logger = logger;

        _shardConectionStringDictionary = new()
        {
            { "MAIN", GetEnvironmentVariable("DB_MAIN") },
            { "RU", GetEnvironmentVariable("DB_RU") },
            { "EU", GetEnvironmentVariable("DB_EU") },
            { "ASIA", GetEnvironmentVariable("DB_ASIA") },
        };

        _shardConnections = _shardConectionStringDictionary
            .ToDictionary(
                kvp => kvp.Key,
                kvp => ConnectionMultiplexer.Connect( kvp.Value )
            );
    }

    private string GetEnvironmentVariable( string name )
    {
        var value = Environment.GetEnvironmentVariable( name );
        if ( string.IsNullOrEmpty( value ) )
        {
            throw new InvalidOperationException( $"Environment variable {name} is not set" );
        }
        return value;
    }

    public void SetRegionShard( string key )
    {
        var mainShard = _shardConnections[ "MAIN" ].GetDatabase();
        var region = mainShard.StringGet( key ).ToString();

        _logger.LogInformation( $"LOOKUP {key}, {region}" );

        _database = _shardConnections[ region ].GetDatabase();
    }

    public void SetRank( string key, double value )
    {
        _database.StringSet( "RANK-" + key, value );
    }

    public RedisValue Get( string key, string prefix )
    {
        return _database.StringGet( prefix + key );
    }
}
