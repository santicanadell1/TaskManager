using System.ComponentModel.DataAnnotations;

namespace Service.Models;

public class ResourceDTO
{
    [Required(ErrorMessage = "Name is required.")]
    public string Name { get; set; }
    
    [Required(ErrorMessage = "LastName is required.")]
    public string Type { get; set; }
    
    [Required(ErrorMessage = "Description is required.")]
    public string Description { get; set; }
    
    [Required(ErrorMessage = "Id is required.")]
    public int? Id { get; set; }
}