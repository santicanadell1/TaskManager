namespace Domain;

public class Notification
{
    public bool read;
    public string description;
    
    public bool Read { get => read; set => read = value; }
    public string Description { get => description; set => description = value; }

    public Notification(bool read, string description)
    {
        this.Read = read;
        this.Description = description;
    }
    
}