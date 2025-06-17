namespace Service.Exceptions.ExporterExeptions;

public class ExporterExeption : Exception
{
    public ExporterExeption(string message) : base(message)
    {
    }

    public override string ToString()
    {
        return $"ExporterExeption: {GetType().Name} - {Message}";
    }
}