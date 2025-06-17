namespace Service.Exceptions.AdminPServiceExceptions;

public class ProjectStartDateException : Exception
{
    public ProjectStartDateException() : base("The Start Date must be greater than the current date")
    {
    }
}