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
    public bool Loading { get; set; } = false;

    public void OnGet(string id)
	{
        _logger.LogDebug( id );
		_shardManager.SetRegionShard( id );
		var rank = _shardManager.Get( id, "RANK-" );
        var similarity = ( int )_shardManager.Get( id, "SIMILARITY-" );
        Similarity = similarity;
        if ( rank != RedisValue.Null )
        {
            Rank = Convert.ToDouble( rank );
            Console.WriteLine( Rank );
            return;
        }
        Loading = true;
    }
}
