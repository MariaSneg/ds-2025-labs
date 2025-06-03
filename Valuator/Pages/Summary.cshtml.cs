using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StackExchange.Redis;
using Valuator.Services;

namespace Valuator.Pages;
public class SummaryModel : PageModel
{
    private readonly ILogger<SummaryModel> _logger;
    private readonly ITextService _textService;

    public SummaryModel( ILogger<SummaryModel> logger, ITextService textService )
    {
        _logger = logger;
        _textService = textService;
    }


    public double Rank { get; set; }
    public double Similarity { get; set; }
    public string Error { get; set; } = "";
    public bool Loading { get; set; } = false;

    public IActionResult OnGet( string id )
    {
        _logger.LogDebug( id );
        _textService.SetRegion( id );

        var userClaim = User.FindFirst( ClaimTypes.Name );
        if ( userClaim == null )
        {
            Error = "Вы не авторизованы";
            return Page();
        }

        string username = userClaim.Value;

        string author = _textService.GetAuthor( id ).ToString();

        if ( username != author )
        {
            Error = "Вы не имеете прав доступа";
            return Page();
        }

        var rankValue = _textService.Get( id, "RANK-" );
        var similarityValue = _textService.Get( id, "SIMILARITY-" );

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
