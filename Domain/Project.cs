using Domain.Exceptions.ProjectExceptions;

namespace Domain;

public class Project
{
    public string description;
    public string name;
    public DateTime startDate;


    public Project()
    {
    }

    public Project(string name, string description, DateTime startDate)
    {
        Name = name;
        Description = description;
        StartDate = startDate;
    }

    public int? Id { get; set; }
    public List<User> Members { get; set; } = new();
    public List<Task> Tasks { get; set; } = new();
    public int? AdminProjectId { get; set; }
    public User AdminProject { get; set; }
    public int? ProjectLeaderId { get; set; }
    public User ProjectLeader { get; set; }

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
            startDate = value;
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

    public void SetProjectLeader(User user)
    {
        ProjectLeader = user;
    }
}