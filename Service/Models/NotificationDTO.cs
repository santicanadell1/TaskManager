using System.ComponentModel.DataAnnotations;

namespace Service.Models;

public class NotificationDTO
{
    [Required(ErrorMessage = "IsRead is required.")]
    public bool? Read { get; set; }

    [Required(ErrorMessage = "Description is required.")]
    public string Description { get; set; }

    [Required(ErrorMessage = "Project is required.")]
    public ProjectDTO Project { get; set; }

    public int? Id { get; set; }
}