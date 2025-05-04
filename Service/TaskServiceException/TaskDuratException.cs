namespace Domain.Exceptions
{
    public class TaskDuratException : TaskServiceException
    {
        public TaskDuratException() : base("Task duration must be greater than zero.") { }
    }
}
