namespace Domain.Exceptions
{
    public class TaskEndDateException : TaskException
    {
        public TaskEndDateException() 
            : base("Expected end date cannot be before the expected start date.") { }
    }
}