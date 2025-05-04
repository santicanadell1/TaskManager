using System.ComponentModel.DataAnnotations;
using Domain;

namespace Service.Models;
public class NotificationDTO
{
    [Required(ErrorMessage = "Read is required.")]
    public bool? Read { get; set; }
}