using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Valuator.DTOs;
using Valuator.Utils;

namespace Valuator.Pages;

public class AuthorizationModel : PageModel
{
    [BindProperty]
    public LoginDto Input { get; set; }
    private IShardManager _shardManager;
    private IPasswordHasher _passwordHasher;
    private ILogger<AuthorizationModel> _logger;

    public AuthorizationModel( ILogger<AuthorizationModel> logger,IShardManager shardManager, IPasswordHasher passwordHasher )
    {
        _shardManager = shardManager;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }
    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPost()
    {
        if ( !ModelState.IsValid )
        {
            return Page();
        }

        var user = await _shardManager.GetUser( Input.Username );

        if ( user is null )
            return Page();

        if ( !_passwordHasher.Verify( Input.Password, user.Password ) )
        {
            return Page();
        }

        var claims = new List<Claim> { new( ClaimTypes.Name, user.Username ) };
        ClaimsIdentity claimsIdentity = new ClaimsIdentity( claims, "Cookies" );
        await HttpContext.SignInAsync( CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal( claimsIdentity ) );

        return Redirect( "/" );
    }

}
