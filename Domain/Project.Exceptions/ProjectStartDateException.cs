namespace Domain.Exceptions.ProjectExceptions;

public class ProjectStartDateException : ProjectException
{
    public ProjectStartDateException()
        : base("Project start date cannot be the default value.")
    {
    }
}