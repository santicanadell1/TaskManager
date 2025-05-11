namespace Domain.Exceptions
{
    public class EmptyTaskListException : CpmServiceException
    {
        public EmptyTaskListException() : base("La lista de tareas no puede estar vacía")
        {
        }
    }
}