namespace Service.Exceptions.LeaderPServiceException;

public class UnableToExportProject : LeaderPServiceException
{
    public UnableToExportProject() :
        base("Error al exportar proyectos")
    {
    }
}