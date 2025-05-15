namespace Domain.Exceptions.ProjectExceptions;

public class ProjectNameException : ProjectException
{
    public ProjectNameException()
        : base("Project name cannot be null, empty, or whitespace.")
    {
    }
}