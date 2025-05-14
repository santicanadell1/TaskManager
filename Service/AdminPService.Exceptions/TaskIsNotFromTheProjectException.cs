namespace Domain.Exceptions.NotificationExceptions;

public class TaskIsNotFromTheProjectException:Exception
{
    public TaskIsNotFromTheProjectException() : base("The task is not from the project."){}
}