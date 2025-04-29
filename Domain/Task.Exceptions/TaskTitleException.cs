namespace Domain.Exceptions
{
    public class TaskTitleException : TaskException
    {
        public TaskTitleException() 
            : base("Title cannot be empty or whitespace.") { }
    }
}