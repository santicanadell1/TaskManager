namespace Domain.Exceptions.NotificationExceptions
{
    public class NullTaskListException : CpmServiceException
    {
        public NullTaskListException() : base("La lista de tareas no puede ser nula")
        {
        }
    }
}