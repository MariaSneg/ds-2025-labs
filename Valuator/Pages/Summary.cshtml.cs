using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StackExchange.Redis;

namespace Valuator.Pages;
public class SummaryModel : PageModel
{
    private readonly ILogger<SummaryModel> _logger;
    private readonly IShardManager _shardManager;

    public SummaryModel( ILogger<SummaryModel> logger, IShardManager shardManager )
    {
        _logger = logger;
        _shardManager = shardManager;
    }

    public double Rank { get; set; }
    public double Similarity { get; set; }
    public string Error { get; set; } = "";
    public bool Loading { get; set; } = false;

    public IActionResult OnGet( string id )
    {
        _logger.LogDebug( id );
        _shardManager.SetRegionShard( id );

        

        var rankValue = _shardManager.Get( id, "RANK-" );
        var similarityValue = _shardManager.Get( id, "SIMILARITY-" );

        if ( similarityValue == RedisValue.Null )
        {
            Similarity = 0;
        }
        else
        {
            Similarity = ( int )similarityValue;
        }

        if ( rankValue != RedisValue.Null )
        {
            Rank = Convert.ToDouble( rankValue );
            return Page();
        }

        Loading = true;
        return Page();
    }
}
