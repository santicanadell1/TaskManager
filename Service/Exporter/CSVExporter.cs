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

        // Los proyectos ya vienen ordenados por fecha de inicio desde ExporterBase
        foreach (var project in projects)
        {
            // Línea del proyecto
            strings.AppendLine($"{EscapeCsvField(project.Name)},{project.StartDate:dd/MM/yyyy}");

            List<TaskDTO> tasks = _taskService.GetTasks(project.Name);

            // Tareas por título (decreciente) según especificación
            foreach (var task in tasks.OrderByDescending(t => t.Title))
            {
                // Línea de la tarea
                strings.AppendLine(
                    $"{EscapeCsvField(task.Title)},{task.StartDate:dd/MM/yyyy},{(task.IsCritical ? "S" : "N")}");

                // Líneas de recursos (cada recurso en su propia línea)
                if (task.Resources?.Any() == true)
                {
                    foreach (var resource in task.Resources)
                    {
                        strings.AppendLine($"{EscapeCsvField(resource.Name ?? "")}");
                    }
                }
            }
        }

        return strings.ToString();
    }

    private string EscapeCsvField(string field)
    {
        if (string.IsNullOrEmpty(field))
            return "";

        // Si contiene comas, saltos de línea o comillas, envolver en comillas dobles
        if (field.Contains(',') || field.Contains('\n') || field.Contains('\r') || field.Contains('"'))
        {
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }

        return field;
    }
}