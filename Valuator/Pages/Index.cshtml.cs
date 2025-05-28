using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Valuator.Services;

namespace Valuator.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IShardManager _shardManager;
    private readonly IRabbitmqService _service;
    private readonly Dictionary<string, string> _countryRegions = new()
    {
        [ "Russia" ] = "RU",
        [ "France" ] = "EU",
        [ "Germany" ] = "EU",
        [ "UAE" ] = "ASIA",
        [ "India" ] = "ASIA"
    };

    public string Port { get; private set; }

    public IndexModel( ILogger<IndexModel> logger, IShardManager shardManager, IRabbitmqService rabbitmqService )
    {
        _logger = logger;
        _shardManager = shardManager;
        _service = rabbitmqService;
    }

    public void OnGet()
    {
        Port = Environment.GetEnvironmentVariable( "EXTERNAL_PORT" ) ?? "NO PORT";
    }
    
    public IActionResult OnPost(string text, string country, CancellationTokenSource cts )
    {
        _logger.LogDebug( text );

        if(string.IsNullOrEmpty( text ) )
        {
            return Page();
        }
        
        string id = Guid.NewGuid().ToString();
        _shardManager.SetToMain( id, _countryRegions[ country ] );

		_shardManager.SetRegionShard( id );

		int similarity = _shardManager.CheckSimilarity( text );
        _shardManager.SetSimilarity( id, similarity );

        _shardManager.SetText( id, text );

        var userClaim = User.FindFirst( ClaimTypes.Name );
        if ( userClaim == null )
        {
            return RedirectToPage( "/Authorization" );
        }

        string username = userClaim.Value;

        _shardManager.SetAuthor( id, username );


        _service.SendRankMessage( id, cts );
        _service.SendSimilarityMessage( id, similarity, cts );

        return Redirect( $"summary?id={id}" );
    }
}
