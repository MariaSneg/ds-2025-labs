using Microsoft.AspNetCore.Mvc.RazorPages;
using StackExchange.Redis;

namespace Valuator.Pages;
public class SummaryModel : PageModel
{
    private readonly ILogger<SummaryModel> _logger;
    private readonly IDatabase _database;

    public SummaryModel( ILogger<SummaryModel> logger, IConnectionMultiplexer redis )
    {
        _logger = logger;
        _database = redis.GetDatabase();
    }

    public double Rank { get; set; }
    public double Similarity { get; set; }

    public void OnGet(string id)
    {
        _logger.LogDebug(id);

        Rank = ( double )_database.StringGet( $"RANK-{id}" );
        Similarity = ( int )_database.StringGet( $"SIMILARITY-{id}" );
    }
}
