namespace Domain.Exceptions
{
    public class TaskDurationException : TaskServiceException
    {
        public TaskDurationException() : base("Task duration must be greater than zero.") { }
    }
}
