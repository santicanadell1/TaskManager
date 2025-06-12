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
        strings.AppendLine("Proyecto, Fecha de Inicio, Tarea, Fecha de Inicio, Duración, Crítico, Recursos");

        //Fecha de Inicio (creciente)
        foreach (var project in projects.OrderBy(p => p.StartDate))
        {
            List<TaskDTO> tasks = _taskService.GetTasks(project.Name);

            // tareas por Título (decreciente)
            foreach (var task in tasks.OrderByDescending(t => t.Title))
            {
                // recursos asociados a la tarea
                var recursos = string.Join(",", task.Resources.Select(r => r.Name));

                strings.AppendLine($"{project.Name},{project.StartDate:dd/MM/yyyy},{task.Title},{task.StartDate:dd/MM/yyyy},{task.Duration},{(task.IsCritical ? "S" : "N")},{recursos}");
            }
        }

        return strings.ToString();
    }
}