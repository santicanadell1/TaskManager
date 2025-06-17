using Domain.Exceptions.NotificationExceptions;

namespace Domain;

public class Notification
{
    public string description;
    public Project project;
    public bool read;

    public Notification(bool isRead, string description, Project project)
    {
        IsRead = isRead;
        Description = description;
        Project = project;
    }

    public Notification()
    {
    }

    public int? Id { get; set; }

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