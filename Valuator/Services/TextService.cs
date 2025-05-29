using StackExchange.Redis;

namespace Valuator.Services;

public class TextService : ITextService
{
    private readonly IShardRouter _router;
    private IDatabase _currentShard;

    public TextService( IShardRouter router )
    {
        _router = router;
        _currentShard = router.GetMainShard();
    }

    public void SetRegion( string key )
    {
        _currentShard = _router.GetShardByKey( key );
    }

    public void SetText( string key, string value ) => Set( "TEXT-", key, value );
    public void SetRank( string key, double value ) => Set( "RANK-", key, value );
    public void SetSimilarity( string key, int value ) => Set( "SIMILARITY-", key, value );
    public void SetAuthor( string key, string value ) => Set( "AUTHOR-", key, value );

    public RedisValue Get( string key, string prefix ) => _currentShard.StringGet( prefix + key );
    public RedisValue GetAuthor( string id ) => Get( id, "AUTHOR-" );

    private void Set( string prefix, string key, RedisValue value )
    {
        _currentShard.StringSet( prefix + key, value );
    }

    public async Task<int> CheckSimilarity( string text )
    {
        foreach ( var region in new[] { "RU", "EU", "ASIA" } )
        {
            var db = _router.GetShardByRegion( region );
            var server = db.Multiplexer.GetServer( db.Multiplexer.GetEndPoints()[ 0 ] );

            await foreach ( var key in server.KeysAsync( pattern: "TEXT-*" ) )
            {
                if ( ( await db.StringGetAsync( key ) ) == text )
                {
                    return 1;
                }
            }
        }

        return 0;
    }

    public void SetToMain(string key, string region)
    {
        _currentShard = _router.GetMainShard();
        Set("", key, region);
    }
}
