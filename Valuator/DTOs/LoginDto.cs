using System.ComponentModel.DataAnnotations;

namespace Valuator.DTOs;

public class LoginDto
{
    [Required( ErrorMessage = "Login is required" )]
    [Display( Name = "Login" )]
    public string Username { get; set; }

    [Required( ErrorMessage = "Password is required" )]
    [DataType( DataType.Password )]
    [Display( Name = "Password" )]
    public string Password { get; set; }
}
