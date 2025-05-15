using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Valuator.DTOs;
using Valuator.Utils;

namespace Valuator.Pages;

public class RegistrationModel : PageModel
{
    [BindProperty]
    public CreateUserDto Input { get; set; }
    private IShardManager _shardManager;
    private IPasswordHasher _passwordHasher;

    public RegistrationModel( IShardManager shardManager, IPasswordHasher passwordHasher )
    {
        _shardManager = shardManager;
        _passwordHasher = passwordHasher;
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
        var userExists = await _shardManager.UserExists( Input.Username );
        if ( userExists )
        {
            ModelState.AddModelError( string.Empty, "Username or email already exists." );
            return Page();
        }

        await _shardManager.AddUser( new Models.User
        {
            Username = Input.Username,
            Password = _passwordHasher.Hash( Input.Password )
        } );

        return Redirect( "/authorization" );
    }

}
