namespace Service.Exceptions.ExporterExeptions;

public class NullProjectsCanNotBeImported : ExporterExeption
{
    public NullProjectsCanNotBeImported() :
        base("Error al exportar proyectos")
    {
    }
}