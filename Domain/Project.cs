namespace Domain;

public class Project
{
    public string name;
    public string description;
    public DateTime startDate;
    public List<User> members;
    public List<Task> tasks;
    public User adminProject;
    public string Name{get;set;}
    public string Description{get;set;}
    public DateTime StartDate{get;set;}
    public List<User> Members{get;set;}
    public List<Task> Tasks{get;set;}
    public User AdminProject{get;set;}
    public Project(){}
}