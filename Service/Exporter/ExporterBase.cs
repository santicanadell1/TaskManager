using Service.Exceptions.ExporterExeptions;
using Service.Models;

public abstract class ExporterBase : IExporter
{
    public string Export(List<ProjectDTO> projects)
    {
        if (projects == null)
            throw new NullProjectsCanNotBeImported();

        List<ProjectDTO> orderedProjects = OrderProjects(projects);
        var result = ExportData(orderedProjects);
        return result;
    }

    protected abstract string ExportData(List<ProjectDTO> projects);

    protected virtual List<ProjectDTO> OrderProjects(List<ProjectDTO> projects)
    {
        return projects
            .Where(p => p != null)
            .OrderBy(p => p.StartDate)
            .ToList();
    }
}