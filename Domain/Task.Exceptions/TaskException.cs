namespace Domain.Exceptions.NotificationExceptions
{
    public class TaskException : Exception
    {
        public TaskException(string message) : base(message) { }

        public override string ToString()
        {
            return $"TaskException: {this.GetType().Name} - {this.Message}";
        }
    }
}