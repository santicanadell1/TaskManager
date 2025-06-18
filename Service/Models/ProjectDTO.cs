using System.ComponentModel.DataAnnotations;

namespace Service.Models;

public class ProjectDTO
{
    [Required(ErrorMessage = "Project name is required.")]
    public string Name { get; set; }

    [Required(ErrorMessage = "Project description is required.")]
    public string Description { get; set; }

    [Required(ErrorMessage = "Project start date is required.")]
    public DateTime StartDate { get; set; }

    public UserDTO? AdminProyect { get; set; }

    public List<UserDTO> Members { get; set; }

    public List<TaskDTO> Tasks { get; set; }
    public UserDTO ProjectLeader { get; set; }

    public int? Id { get; set; }
}