namespace Domain.Exceptions.NotificationExceptions
{
    public class EmptyTaskListException : CpmServiceException
    {
        public EmptyTaskListException() : base("La lista de tareas no puede estar vacía")
        {
        }
    }
}