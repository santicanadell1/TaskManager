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
    public string Title
    {
        get => title;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Title cannot be empty or white spaces");
            }
            title = value;
        }
    }
    public string Description
    {
        get => description;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Description cannot be empty or white spaces");
            }
            description = value;
        }
    }
    public DateTime ExpectedStartDate { get; set; }
    public DateTime ExpectedEndDate
    {
        get => expectedEndDate;
        set
        {
            if (value < this.ExpectedStartDate)
            {
                throw new ArgumentException("Expected end date cannot be before expected start date");
            }
            expectedEndDate = value;
        }
    }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int Duration
    {
        get => duration;
        set
        {
            if (value <= 0)
            {
                throw new ArgumentException("Duration cannot be zero or negative");
            }
            duration = value;
        }
    }
    public List<Task> PreviousTasks { get; set; }
    public State State { get; set; }

}