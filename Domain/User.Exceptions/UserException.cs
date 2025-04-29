namespace Domain.Exceptions
{
    public class UserException : Exception
    {
        public UserException(string message) : base(message) { }

        public override string ToString()
        {
            return $"UserException: {this.GetType().Name} - {this.Message}";
        }
    }
}