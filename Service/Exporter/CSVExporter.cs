using System.Text;
using DataAccess;
using Service;
using Service.Models;

public class CSVExporter : ExporterBase
{
    private readonly TaskService _taskService;

    public CSVExporter(IRepositoryManager repositoryManager)
    {
        _taskService = new TaskService(repositoryManager, new CpmService());
    }

    protected override string ExportData(List<ProjectDTO> projects)
    {
        var strings = new StringBuilder();

        foreach (var project in projects)
        {
            strings.AppendLine($"{EscapeCsvField(project.Name)},{project.StartDate:dd/MM/yyyy}");

            List<TaskDTO> tasks = _taskService.GetTasks(project.Name);

            foreach (var task in tasks.OrderByDescending(t => t.Title))
            {
                strings.AppendLine(
                    $"{EscapeCsvField(task.Title)},{task.StartDate:dd/MM/yyyy},{(task.IsCritical ? "S" : "N")}");

                if (task.Resources?.Any() == true)
                    foreach (var resource in task.Resources)
                        strings.AppendLine($"{EscapeCsvField(resource.Name ?? "")}");
            }
        }

        return strings.ToString();
    }

    private string EscapeCsvField(string field)
    {
        if (string.IsNullOrEmpty(field))
            return "";

        if (field.Contains(',') || field.Contains('\n') || field.Contains('\r') || field.Contains('"'))
            return $"\"{field.Replace("\"", "\"\"")}\"";

        return field;
    }
}