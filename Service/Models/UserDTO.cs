using System.ComponentModel.DataAnnotations;
using Domain;

namespace Service.Models;

public class UserDTO
{
    [Required(ErrorMessage = "Name is required.")]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "LastName is required.")]
    public string LastName { get; set; }

    [Required(ErrorMessage = "Email is required.")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Birthday is required.")]
    public DateTime Birthday { get; set; }

    [Required(ErrorMessage = "Password is required.")]
    public string Password { get; set; }

    public List<RolDTO> Roles { get; set; }

    public List<int> Tasks { get; set; }

    public List<NotificationDTO> Notifications { get; set; }
}