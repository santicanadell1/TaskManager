namespace Domain.Exceptions
{
    public class TaskDurationException : TaskException
    {
        public TaskDurationException() 
            : base("Duration cannot be zero or negative.") { }
    }
}