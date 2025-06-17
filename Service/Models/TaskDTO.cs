using System.ComponentModel.DataAnnotations;

namespace Service.Models;

public class TaskDTO
{
    [Required(ErrorMessage = "Title is required.")]
    public string Title { get; set; }

    [Required(ErrorMessage = "Description is required.")]
    public string Description { get; set; }

    [Required(ErrorMessage = "ExpectedStartDate is required.")]
    public DateTime ExpectedStartDate { get; set; }

    [Required(ErrorMessage = "Duration is required.")]
    public int Duration { get; set; }

    public List<TaskDTO> PreviousTasks { get; set; }

    public List<TaskDTO> SameTimeTasks { get; set; }

    [Required(ErrorMessage = "State is required.")]
    public StateDTO State { get; set; } = StateDTO.TODO;

    public int? Id { get; set; }

    public List<ResourceDTO> Resources { get; set; } = new();

    public bool IsCritical { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime LatestStart { get; set; }
    public DateTime LatestFinish { get; set; }
    public TimeSpan Slack { get; set; }
}