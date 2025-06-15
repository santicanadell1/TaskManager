using Domain.Exceptions.ResourceExceptions;

namespace Domain;

public class Resource
{
    public string description;
    public string name;
    public string type;
    public bool ConcurrentUsage{ get; set; }
    public int? Id { get; set; }
    
    public Project? Project { get; set; }

    public Resource(string name, string type, string description, bool concurrentUsage = false, Project? project = null)
    {
        Name = name;
        Type = type;
        Description = description;
        ConcurrentUsage = concurrentUsage;
        Project = project;
    }
    
    public Resource(){}

    public string Name
    {
        get => name;
        set => name = string.IsNullOrEmpty(value) ? throw new ResourceNameException() : value;
    }

    public string Type
    {
        get => type;
        set => type = string.IsNullOrEmpty(value) ? throw new ResourceTypeException() : value;
    }

    public string Description 
    { 
        get => description;
        set => description = value;
    }
   
}