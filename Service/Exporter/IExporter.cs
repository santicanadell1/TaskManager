using Service.Models;

public interface IExporter
{
    string Export(List<ProjectDTO> projects);
}