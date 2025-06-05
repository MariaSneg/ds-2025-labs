using StackExchange.Redis;

namespace RankCalculator;
public interface IShardManager
{
    public void SetRegionShard( string key );
	public void SetRank( string key, double value );
    public RedisValue Get( string key, string prefix );
}