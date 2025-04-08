using StackExchange.Redis;

namespace Valuator.Repositories;

public class ValuatorRepository : IValuatorRepository
{
    private readonly IDatabase _database;
    private readonly IServer _server;

    public ValuatorRepository( IConnectionMultiplexer redis )
    {
        _database = redis.GetDatabase();
        _server = redis.GetServer( redis.GetEndPoints().First() );
    }

    public void AddText( string id, string text )
    {
        string textKey = "TEXT-" + id;
        _database.StringSet( textKey, text );
    }

    public void AddSimilarity( string id, int similarity )
    {
        string similarityKey = "SIMILARITY-" + id;
        _database.StringSet( similarityKey, similarity );
    }

    public void AddRank( string id, double rank )
    {
        string rankKey = "RANK-" + id;
        _database.StringSet( rankKey, rank );
    }

    public int GetSimilarityById( string id )
    {
        string similarityKey = "SIMILARITY-" + id;
        return ( int )_database.StringGet( similarityKey );
    }

    public double GetRankById( string id )
    {
        string rankKey = "RANK-" + id;
        return ( double )_database.StringGet( rankKey );
    }

    public int CheckSimilarity( string text )
    {
        var keys = _server.Keys( pattern: "TEXT-*" );
        foreach ( var key in keys )
        {
            if ( _database.StringGet( key ) == text )
            {
                return 1;
            }
        }
        return 0;
    }
}
