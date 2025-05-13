using StackExchange.Redis;

namespace ShardManager;
public interface IShardManager
{
    public void SetRank( string key, double value );
    public void SetSimilarity( string key, int value );
    public void SetText( string key, string value );
    public void SetToMain( string key, string value );
    public RedisValue Get( string key, string prefix );
}
