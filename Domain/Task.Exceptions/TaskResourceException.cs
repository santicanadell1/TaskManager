namespace Domain.Exceptions
{
    public class TaskResourceException : TaskException
    {
        public TaskResourceException(string message) 
            : base(message) { }
    }
}