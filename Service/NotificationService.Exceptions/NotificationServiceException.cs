namespace Domain.Exceptions.NotificationExceptions
{
    public class NotificationServiceException : Exception
    {
        public NotificationServiceException(string message) : base(message)
        {
        }

        public override string ToString()
        {
            return $"NotificationServiceException: {this.GetType().Name} - {this.Message}";
        }
    }
}