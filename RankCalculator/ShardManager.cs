using StackExchange.Redis;

namespace RankCalculator;

public class ShardManager : IShardManager
{
    private Dictionary<string, string> _shardConectionStringDictionary = new Dictionary<string, string>();
    private ILogger<ShardManager> _logger;
    private IDatabase _database;


	public ShardManager( ILogger<ShardManager> logger, IConfiguration configuration )
    {
        _logger = logger;
		_shardConectionStringDictionary.Add( "MAIN", configuration[ "RedisConnections:MAIN" ] ?? "redis_main:6379" );
		_shardConectionStringDictionary.Add( "RU", configuration[ "RedisConnections:RU" ] ?? "redis_ru:6379" );
		_shardConectionStringDictionary.Add( "EU", configuration[ "RedisConnections:EU" ] ?? "redis_eu:6379" );
		_shardConectionStringDictionary.Add( "ASIA", configuration[ "RedisConnections:ASIA" ] ?? "redis_asia:6379" );
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
        var mainShard = ConnectionMultiplexer.Connect( _shardConectionStringDictionary[ "MAIN" ] ).GetDatabase();
        var region = mainShard.StringGet( key ).ToString();

		_logger.LogInformation( $"LOOKUP {key}, {region}" );

		_database =  ConnectionMultiplexer.Connect( _shardConectionStringDictionary[ region ] ).GetDatabase();
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
        var mainShard = ConnectionMultiplexer.Connect( _shardConectionStringDictionary[ "MAIN" ] ).GetDatabase();
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

            var shard = ConnectionMultiplexer.Connect( _shardConectionStringDictionary[ region ] ).GetDatabase();

            var server = ConnectionMultiplexer.Connect( _shardConectionStringDictionary[ region ] ).GetServer( _shardConectionStringDictionary[ region ] );
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
