using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Valuator.Pages;

public class RegistrationModel : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; }

    public class InputModel
    {
        [Required( ErrorMessage = "Login is required" )]
        [Display( Name = "Login" )]
        public string Username { get; set; }

        [Required( ErrorMessage = "Password is required" )]
        [DataType( DataType.Password )]
        [Display( Name = "Password" )]
        public string Password { get; set; }

        [DataType( DataType.Password )]
        [Display( Name = "Confirm your password" )]
        [Compare( "Password", ErrorMessage = "Пароли не совпадают" )]
        public string ConfirmPassword { get; set; }
    }

    public void OnGet()
    {
    }
}
