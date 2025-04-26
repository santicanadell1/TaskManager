using Domain.Test;

namespace Domain;

public class Task
{
    private string title;
    private string description;
    private DateTime expectedStartDate;
    private DateTime expectedEndDate;
    private DateTime startDate;
    private DateTime endDate;
    private int duration;
    private List<Task> previousTasks;
    private State state;
    
    public Task(string title, string description, DateTime startDate, DateTime expectedEndDate, int duration, List<Task> previousTasks)
    {
        this.Title = title;
        this.Description = description;
        this.ExpectedStartDate = startDate;
        this.ExpectedEndDate = expectedEndDate;
        this.Duration = duration;
        this.PreviousTasks = previousTasks;
        this.State = State.TODO;
    }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime ExpectedStartDate { get; set; }
    public DateTime ExpectedEndDate { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int Duration { get; set; }
    public List<Task> PreviousTasks { get; set; }
    public State State { get; set; }

    

    

    

    

    

    
}