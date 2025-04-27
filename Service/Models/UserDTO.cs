using System.ComponentModel.DataAnnotations;

namespace Service.Models;

public class UserDTO
{
    [Required(ErrorMessage = "Name is required.")]
    public string FirstName { get; set; }
}