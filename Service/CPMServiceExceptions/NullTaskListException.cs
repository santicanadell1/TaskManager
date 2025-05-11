namespace Domain.Exceptions
{
    public class NullTaskListException : CpmServiceException
    {
        public NullTaskListException() : base("La lista de tareas no puede ser nula")
        {
        }
    }
}