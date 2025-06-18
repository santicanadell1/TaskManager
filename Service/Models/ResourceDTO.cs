using System.ComponentModel.DataAnnotations;

namespace Service.Models;

public class ResourceDTO
{
    [Required(ErrorMessage = "Name is required.")]
    public string Name { get; set; }

    [Required(ErrorMessage = "Type is required.")]
    public string Type { get; set; }

    [Required(ErrorMessage = "Description is required.")]
    public string Description { get; set; }

    public bool ConcurrentUsage { get; set; }

    public ProjectDTO? Project { get; set; }

    public int? Id { get; set; }
}