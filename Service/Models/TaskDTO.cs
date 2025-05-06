using System.ComponentModel.DataAnnotations;
using Domain;

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
    
    public List<Domain.Task> PreviousTasks { get; set; }
    
    public List<Domain.Task> SameTimeTasks { get; set; }
    
    [Required(ErrorMessage = "State is required.")]
    public State State { get; set; } = State.TODO;

    

}