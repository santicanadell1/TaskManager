namespace Domain.Exceptions
{
    public class TaskStartDateException : TaskServiceException
    {
        public TaskStartDateException() : base("Task start date cannot be in the past.") { }
    }
}