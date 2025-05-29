using StackExchange.Redis;

namespace Valuator.Services;

public interface ITextService
{
    void SetText( string key, string value );
    void SetRank( string key, double value );
    void SetSimilarity( string key, int value );
    void SetAuthor( string key, string value );
    RedisValue Get( string key, string prefix );
    RedisValue GetAuthor( string id );
    Task<int> CheckSimilarity( string inputText );
    void SetRegion( string key );
    void SetToMain( string key, string region );
}
