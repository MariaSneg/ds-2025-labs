using StackExchange.Redis;

namespace ShardManager;

public class RedisShardManager : IShardManager
{
    private Dictionary<string, string> _shardConectionStringDictionary;

    public RedisShardManager()
    {
        _shardConectionStringDictionary = new()
        {
            { "MAIN", "redis_main:6000" },
            { "RU", "redis_ru:6001" },
            { "EU", "redis_eu:6002" },
            { "ASIA", "redis_asia:6003" },
        };
    }

    private IDatabase GetRegionShard(string key)
    {
        var mainShard = ConnectionMultiplexer.Connect( _shardConectionStringDictionary["MAIN"]).GetDatabase();
        var region = mainShard.StringGet( key ).ToString();
        return  ConnectionMultiplexer.Connect( _shardConectionStringDictionary[ region ] ).GetDatabase();
    }

    public void SetRank( string key, double value )
    {
        var shard = GetRegionShard( key );
        shard.StringSet("RANK-" + key, value);
    }

    public void SetSimilarity( string key, int value )
    {
        var shard = GetRegionShard( key );
        shard.StringSet( "SIMILARITY-" + key, value );
    }

    public void SetText( string key, string value )
    {
        var shard = GetRegionShard( key );
        shard.StringSet( "TEXT-" + key, value );
    }

    public void SetToMain( string key, string value )
    {
        var mainShard = ConnectionMultiplexer.Connect( _shardConectionStringDictionary[ "MAIN" ] ).GetDatabase();
        mainShard.StringSet( key, value );
    }

    public RedisValue Get( string key, string prefix )
    {
        var shard = GetRegionShard( key );
        return shard.StringGet( prefix + key );
    }
}
