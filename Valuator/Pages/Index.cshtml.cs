using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StackExchange.Redis;

namespace Valuator.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IDatabase _database;
    private readonly IServer _server;

    public IndexModel( ILogger<IndexModel> logger, IConnectionMultiplexer redis )
    {
        _logger = logger;
        _database = redis.GetDatabase();
        _server = redis.GetServer( redis.GetEndPoints().First() );
    }

    public void OnGet()
    {

    }

    public IActionResult OnPost(string text)
    {
        _logger.LogDebug(text);

        string id = Guid.NewGuid().ToString();

        if ( !string.IsNullOrEmpty( text ) )
        {
            string similarityKey = "SIMILARITY-" + id;
            int similaity = CheckSimilaity( text, id );
            _database.StringSet( similarityKey, similaity );

            string rankKey = "RANK-" + id;
            _database.StringSet( rankKey, CalculateRank( text ) );

            if ( similaity == 0 )
            {
                string textKey = "TEXT-" + id;
                _database.StringSet( textKey, text );
            }
        }

        return Redirect($"summary?id={id}");
    }

    private double CalculateRank(string text)
    {
        if(string.IsNullOrEmpty(text)) 
            return 0;
        int totalCount = text.Length;

        int nonLetterCount = text.Count( c => !char.IsLetter(c));

        double rank = (nonLetterCount * 1.0) / totalCount;

        return Math.Round( rank, 3 );
    }

    private int CheckSimilaity(string text, string id)
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
