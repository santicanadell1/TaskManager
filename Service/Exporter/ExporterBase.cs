using Service.Models;

public abstract class ExporterBase : IExporter
{
    public string Export(List<ProjectDTO> projects)
    {
        var orderedProjects = OrderProjects(projects);
        var result = ExportData(orderedProjects);
        return result;
    }

    protected abstract string ExportData(List<ProjectDTO> projects);

    protected List<ProjectDTO> OrderProjects(List<ProjectDTO> projects)
    {
        return projects.OrderBy(p => p.StartDate).ToList();
    }
}