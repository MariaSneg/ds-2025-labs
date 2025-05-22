using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StackExchange.Redis;
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

    public IActionResult OnGet( string id )
    {
        _logger.LogDebug( id );
        RedisValue rank = _repository.GetRankById( id );
        var similarity = ( int )_repository.GetSimilarityById( id );
        if ( similarity == RedisValue.Null )
        {
            Similarity = 0;
        }
        else
        {
            Similarity = ( int )similarity;
        }

        if ( rank != RedisValue.Null )
        {
            Rank = Convert.ToDouble( rank );
            return Page();
        }

        Rank = -1;
        Loading = true;
        return Page();
    }
}
