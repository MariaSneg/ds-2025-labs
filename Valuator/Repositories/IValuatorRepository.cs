using StackExchange.Redis;

namespace Valuator.Repositories;

public interface IValuatorRepository
{
    void AddText( string id, string text );
    void AddSimilarity( string id, int similarity );
    void AddRank( string id, double rank );
    int GetSimilarityById( string id );
    RedisValue GetRankById( string id );
    int CheckSimilarity( string text );
}
