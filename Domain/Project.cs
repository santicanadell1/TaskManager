using Domain.Exceptions.ProjectExceptions;

namespace Domain;

public class Project
{
    private string description;
    private string name;
    private DateTime startDate;
    public List<User> Members { get; set; } = new();
    public List<Task> Tasks { get; set; } = new();
    public User AdminProject { get; set; }
    public Project()
    {
    }

    public Project(string name, string description, DateTime startDate)
    {
        Name = name;
        Description = description;
        StartDate = startDate;
    }

    public string Name
    {
        get => name;
        set
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ProjectNameException();

            name = value;
        }
    }

    public string Description
    {
        get => description;
        set
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ProjectDescriptionException();

            description = value;
        }
    }

    public DateTime StartDate
    {
        get => startDate;
        set
        {
            if (value == default) throw new ProjectStartDateException();

            if (value < DateTime.Today)
                throw new ProjectException("The Start Date must be greater than the current date");

            startDate = value;
        }
    }
    
    public string? Id
    {
        get => Name;  
        set
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ProjectNameException();
            Name = value;  
        }
    }   
    
    public void AddMember(User user)
    {
        Members.Add(user);
    }

    public void AddTask(Task task)
    {
        Tasks.Add(task);
    }
}