namespace Domain.Exceptions.ProjectExceptions;

public class ProjectException : Exception
{
    public ProjectException(string message) : base(message)
    {
    }

    public override string ToString()
    {
        return $"ProjectException: {GetType().Name} - {Message}";
    }
}