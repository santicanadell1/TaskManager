namespace Domain.Exceptions
{
    public class TaskEndDteException : TaskServiceException
    {
        public TaskEndDteException() : base("Task end date must be later than the start date.") { }
    }
}