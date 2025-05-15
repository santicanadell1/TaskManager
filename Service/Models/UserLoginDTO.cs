using System.ComponentModel.DataAnnotations;

namespace Service.Models;

public class UserLoginDTO
{
    public UserLoginDTO(string email, string password)
    {
        Email = email;
        Password = password;
    }

    [Required(ErrorMessage = "Email is required.")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Password is required.")]
    public string Password { get; set; }
}