using Microsoft.AspNetCore.Mvc.RazorPages;
using Valuator.Repositories;

namespace Valuator.Pages;
public class SummaryModel : PageModel
{
    private readonly ILogger<SummaryModel> _logger;
    private readonly IValuatorRepository _repository;

    public SummaryModel( ILogger<SummaryModel> logger, IValuatorRepository repository )
    {
        _logger = logger;
        _repository = repository;
    }

    public double Rank { get; set; }
    public double Similarity { get; set; }
    public bool Loading { get; set; }

    public void OnGet( string id )
    {
        _logger.LogDebug( id );
        var rank = _repository.GetRankById( id );
        var similarity = ( int )_repository.GetSimilarityById( id );
        Similarity = similarity;
        if ( rank != StackExchange.Redis.RedisValue.Null )
        {
            Rank = Convert.ToDouble( rank );
            Console.WriteLine( Rank );
            return;
        }
        Loading = true;
    }
}
