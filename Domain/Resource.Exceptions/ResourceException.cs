namespace Domain.Exceptions
{
    public class ResourceException : Exception
    {
        public ResourceException(string message) : base(message) { }

        public override string ToString()
        {
            return $"ResourceException: {this.GetType().Name} - {this.Message}";
        }
    }
}