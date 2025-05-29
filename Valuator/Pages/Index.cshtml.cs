using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Valuator.Services;

namespace Valuator.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly ITextService _textService;
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

    public IndexModel( ILogger<IndexModel> logger, ITextService textService, IRabbitmqService rabbitmqService )
    {
        _logger = logger;
        _textService = textService;
        _service = rabbitmqService;
    }

    public void OnGet()
    {
        Port = Environment.GetEnvironmentVariable( "EXTERNAL_PORT" ) ?? "NO PORT";
    }
    
    public async Task<IActionResult> OnPost(string text, string country, CancellationTokenSource cts )
    {
        _logger.LogDebug( text );

        if(string.IsNullOrEmpty( text ) )
        {
            return Page();
        }
        
        string id = Guid.NewGuid().ToString();
        _textService.SetToMain( id, _countryRegions[ country ] );

        _textService.SetRegion( id );

		int similarity =await _textService.CheckSimilarity( text );
        _textService.SetSimilarity( id, similarity );

        _textService.SetText( id, text );

        var userClaim = User.FindFirst( ClaimTypes.Name );
        if ( userClaim == null )
        {
            return RedirectToPage( "/Authorization" );
        }

        string username = userClaim.Value;

        _textService.SetAuthor( id, username );


        _service.SendRankMessage( id, cts );
        _service.SendSimilarityMessage( id, similarity, cts );

        return Redirect( $"summary?id={id}" );
    }
}
