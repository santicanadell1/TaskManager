namespace Service.Exceptions.MemberServiceExceptions;

public class TaskCantBeModifiedByUserException : Exception
{
    public TaskCantBeModifiedByUserException() : base("The task can't be modified by the user.")
    {
    }
}