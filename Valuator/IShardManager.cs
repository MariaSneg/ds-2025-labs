using StackExchange.Redis;
using Valuator.Models;

namespace Valuator;

public interface IShardManager
{
    public void SetRegionShard( string key );
	public void SetRank( string key, double value );
    public void SetSimilarity( string key, int value );
    public void SetText( string key, string value );
    public void SetAuthor( string key, string value );
    public void SetToMain( string key, string value );
    public RedisValue Get( string key, string prefix );
    public int CheckSimilarity( string text );
    public RedisValue GetAuthor( string id );
    public Task AddUser( User user );
    public Task<bool> UserExists( string username );
    public Task<User?> GetUser( string username );

}