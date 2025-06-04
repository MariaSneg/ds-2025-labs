using StackExchange.Redis;
using Microsoft.Extensions.Logging;

namespace Valuator;

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

    public void SetSimilarity( string key, int value )
    {
        _database.StringSet( "SIMILARITY-" + key, value );
    }

    public void SetText( string key, string value )
    {
        _database.StringSet( "TEXT-" + key, value );
    }

    public void SetToMain( string key, string value )
    {
        var mainShard = _shardConnections[ "MAIN" ].GetDatabase();
        mainShard.StringSet( key, value );
    }

    public RedisValue Get( string key, string prefix )
    {
        return _database.StringGet( prefix + key );
    }

    public int CheckSimilarity( string text )
    {
        foreach ( var region in _shardConectionStringDictionary.Keys )
        {
            if ( region == "MAIN" )
                continue;

            var multiplexer = _shardConnections[ region ];
            var shard = multiplexer.GetDatabase();
            var server = multiplexer.GetServer( multiplexer.GetEndPoints().First() );

            var keys = server.Keys( pattern: "TEXT-*" );

            foreach ( var key in keys )
            {
                var storedText = shard.StringGet( key );
                if ( storedText == text )
                {
                    return 1;
                }
            }
        }
        return 0;
    }
}
