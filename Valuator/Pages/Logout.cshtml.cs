using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Valuator.Pages;

public class LogoutModel : PageModel
{
    public async Task<IActionResult> OnGet()
    {
        await HttpContext.SignOutAsync( CookieAuthenticationDefaults.AuthenticationScheme );
        return Redirect( "/authorization" );
    }

    public async Task<IActionResult> OnPost()
    {
        await HttpContext.SignOutAsync( CookieAuthenticationDefaults.AuthenticationScheme );
        return Redirect( "/authorization" );
    }

}
