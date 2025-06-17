namespace Service.Exceptions.AdminPServiceExceptions;

public class
    TaskIsNotFromTheProjectException : Exception
{
    public TaskIsNotFromTheProjectException() : base("The task is not from the project.")
    {
    }
}