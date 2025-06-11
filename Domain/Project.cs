using Domain.Exceptions.ProjectExceptions;

namespace Domain;

public class Project
{
    public string description;
    public string name;
    public DateTime startDate;
    public List<User> Members { get; set; } = new();
    public List<Task> Tasks { get; set; } = new();
    public User AdminProject { get; set; }

    public int? AdminProjectId { get; set; }
    public User ProjectLeader { get; set; }


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

    public int? Id { get; set; }


    public void AddMember(User user)
    {
        Members.Add(user);
    }

    public void AddTask(Task task)
    {
        Tasks.Add(task);
    }
    
    public void SetProjectLeader(User user)
    {
        ProjectLeader = user;
    }

}