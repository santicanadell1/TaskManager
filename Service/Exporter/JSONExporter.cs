using DataAccess;
using Newtonsoft.Json;
using Service;
using Service.Models;

public class JSONExporter : ExporterBase
{
    private readonly TaskService _taskService;

    public JSONExporter(IRepositoryManager repositoryManager)
    {
        _taskService = new TaskService(repositoryManager, new CpmService());
    }

    protected override string ExportData(List<ProjectDTO> projects)
    {
        var projectsJson = projects
            .Select(p => new ProjectExportDTO
            {
                Project = p.Name,
                StartDate = p.StartDate.ToString("dd/MM/yyyy"),
                Tasks = _taskService.GetTasks(p.Name)
                    .OrderByDescending(t => t.Title)
                    .Select(t => new TaskExportDTO
                    {
                        Task = t.Title,
                        StartDate = t.StartDate.ToString("dd/MM/yyyy"),
                        Duration = t.Duration,
                        IsCritical = t.IsCritical ? "S" : "N",
                        Resources = t.Resources?.Select(r => r.Name ?? "").ToList() ?? new List<string>()
                    })
                    .ToList()
            })
            .ToList();

        return JsonConvert.SerializeObject(projectsJson, Formatting.Indented);
    }
}