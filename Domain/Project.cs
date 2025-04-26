namespace Domain;

public class Project
{
    public string name;
    public string description;
    public DateTime startDate;
    public List<User> members;
    public List<Task> tasks;
    public User adminProject;
    public string Name
    {
        get => name;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Project name cannot be null, empty, or whitespace.");
            }
            name = value;
        }
    }    
    public string Description
    {
        get => description;
        set
        {
            if (string.IsNullOrWhiteSpace(value))  
            {
                throw new ArgumentException("Project description cannot be null, empty, or whitespace.");
            }
            description = value;
        }
    }
    public DateTime StartDate
    {
        get => startDate;
        set
        {
            if (value == default(DateTime)) 
            {
                throw new ArgumentException("Project start date cannot be the default value.");
            }
            startDate = value;
        }
    }
    public List<User> Members{get;set;}
    public List<Task> Tasks{get;set;}
    public User AdminProject{get;set;}
    public Project(){}
}