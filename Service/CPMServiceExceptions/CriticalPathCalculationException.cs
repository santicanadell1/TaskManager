namespace Service.Exceptions.CPMServiceExceptions;

public class CriticalPathCalculationException : CpmServiceException
{
    public CriticalPathCalculationException(string message) : base($"Error al calcular el camino crítico: {message}")
    {
    }
}