namespace Domain.Exceptions.NotificationExceptions
{
    public class InvalidTaskDependencyException : CpmServiceException
    {
        public InvalidTaskDependencyException(string message) : base($"Dependencia de tarea inválida: {message}")
        {
        }
    }
}