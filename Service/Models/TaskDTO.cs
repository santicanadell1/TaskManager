using System.ComponentModel.DataAnnotations;
using Domain;

namespace Service.Models;
public class TaskDTO
{
    [Required(ErrorMessage = "Title is required.")]
    public string Title { get; set; }
    
    [Required(ErrorMessage = "Description is required.")]
    public string Description { get; set; }
}