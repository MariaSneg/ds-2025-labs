using System.ComponentModel.DataAnnotations;

namespace Valuator.DTOs;

public class CreateUserDto
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
