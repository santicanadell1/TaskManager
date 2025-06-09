using Domain.Exceptions.NotificationExceptions;

namespace Domain;

public class Notification
{
    private string description;
    private Project project;
    private bool read;
    public int? Id { get; set; }

    public Notification(bool isRead, string description, Project project)
    {
        IsRead = isRead;
        Description = description;
        Project = project;
    }

    public Notification()
    {
    }

    public bool IsRead
    {
        get => read;
        set => read = value;
    }

    public string Description
    {
        get => description;
        set
        {
            if (string.IsNullOrWhiteSpace(value)) throw new NotificationDescriptionException();

            description = value;
        }
    }

    public Project Project
    {
        get => project;
        set
        {
            if (value == null) throw new NotificationException("Project is null");
            project = value;
        }
    }

    public void MarkRead()
    {
        read = true;
    }
}