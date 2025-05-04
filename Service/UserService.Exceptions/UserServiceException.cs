namespace Domain.Exceptions
{
    public class UserServiceException : Exception
    {
        public UserServiceException(string message) : base(message) { }

        public override string ToString()
        {
            return $"UserException: {this.GetType().Name} - {this.Message}";
        }
    }
}