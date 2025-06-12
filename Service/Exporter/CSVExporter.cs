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
        strings.AppendLine("Proyecto,Fecha de Inicio,Tarea,Fecha de Inicio,Duración,Crítico,Recursos");

        // Los proyectos ya vienen ordenados por fecha de inicio desde ExporterBase
        foreach (var project in projects)
        {
            List<TaskDTO> tasks = _taskService.GetTasks(project.Name);

            // Tareas por título (decreciente) según especificación
            foreach (var task in tasks.OrderByDescending(t => t.Title))
            {
                var recursos = task.Resources?.Any() == true 
                    ? string.Join(";", task.Resources.Select(r => r.Name ?? "")) 
                    : "";

                var projectName = EscapeCsvField(project.Name);
                var taskTitle = EscapeCsvField(task.Title);
                var resourcesEscaped = EscapeCsvField(recursos);

                strings.AppendLine($"{projectName},{project.StartDate:dd/MM/yyyy},{taskTitle},{task.StartDate:dd/MM/yyyy},{task.Duration},{(task.IsCritical ? "S" : "N")},{resourcesEscaped}");
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