using System.ComponentModel.DataAnnotations;

namespace Service.Models;

public class ResourceDTO
{
    [Required(ErrorMessage = "Name is required.")]
    public string Name { get; set; }
}