namespace Domain.Exceptions.NotificationExceptions
{
    public class AdminSServiceException : Exception
    {
        public AdminSServiceException(string message) : base(message) { }

        public override string ToString()
        {
            return $"AdminSServiceException: {this.GetType().Name} - {this.Message}";
        }
    }
}