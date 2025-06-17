public class ProjectExportDTO
{
    public string Project { get; set; }
    public string StartDate { get; set; }

    public List<TaskExportDTO> Tasks { get; set; }
}