namespace Domain.Exceptions;

public class UserTaskException:Exception
{
    public UserTaskException() : base("The task is already assigned to the user.")
    {
        
    }
}