namespace Domain.Exceptions.NotificationExceptions
{
    public class CircularDependencyException : CpmServiceException
    {
        public CircularDependencyException() : base("Se detectó una dependencia circular en las tareas")
        {
        }
    }
}