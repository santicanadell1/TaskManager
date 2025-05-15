namespace DataAccess.Exceptions.ProjectRepositoryExceptions;

public class DuplicatedProjectsNameException : Exception
{
    public DuplicatedProjectsNameException() : base("The project name already exists.")
    {
    }
}