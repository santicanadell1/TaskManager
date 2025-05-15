namespace DataAccess.Exceptions.ProjectRepositoryExceptions;

public class ProjectNotFoundException : Exception
{
    public ProjectNotFoundException() : base("The project was not found in the repository.")
    {
    }
}