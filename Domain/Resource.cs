namespace Domain;

public class Resource
{
    public string name;
    public string type;
    public string description;
    
    
    public string Name {get => name;
                        set {name = string.IsNullOrEmpty(value) ? throw new ArgumentException("the resource name can not be empty"): value; }
    }
    public string Type {get => type ;
        set { type = string.IsNullOrEmpty(value) ? throw new ArgumentException("the resource type can not be empty"): value; }
    }
    public string Description {get; set;}

    

    public Resource(string name, string type, string description)
    {
        this.Name = name;
        this.Type = type;
        this.Description = description;
    }
}

    