namespace Domain.Exceptions.ProjectExceptions;

public class ProjectDescriptionException : ProjectException
{
    public ProjectDescriptionException()
        : base("Project description cannot be null, empty, or whitespace.")
    {
    }
}