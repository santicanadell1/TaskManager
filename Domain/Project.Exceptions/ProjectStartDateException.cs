namespace Domain.Exceptions.NotificationExceptions
{
    public class ProjectStartDateException : ProjectException
    {
        public ProjectStartDateException() 
            : base("Project start date cannot be the default value.") { }
    }
}