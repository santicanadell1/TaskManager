namespace Domain.Exceptions.NotificationExceptions
{
    public class ResourceTypeException : ResourceException
    {
        public ResourceTypeException() 
            : base("The resource type cannot be empty.") { }
    }
}