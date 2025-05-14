namespace Domain.Exceptions.NotificationExceptions
{
    public class CriticalPathCalculationException : CpmServiceException
    {
        public CriticalPathCalculationException(string message) : base($"Error al calcular el camino crítico: {message}")
        {
        }
    }
}