namespace Domain.Exceptions
{
    public class TaskEndDateException : TaskServiceException
    {
        public TaskEndDateException() : base("Task end date must be later than the start date.") { }
    }
}