namespace Service.Exceptions.CPMServiceExceptions
{
    public class InvalidTaskDependencyException : CpmServiceException
    {
        public InvalidTaskDependencyException(string message) : base($"Dependencia de tarea inválida: {message}")
        {
        }
    }
}