using StackExchange.Redis;

namespace Valuator;

public interface IShardRouter
{
    IDatabase GetShardByKey( string key );
    IDatabase GetMainShard();
    IDatabase GetShardByRegion( string region );
}
