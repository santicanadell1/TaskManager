public class TaskExportDTO
{
    public string Task { get; set; }
    public string StartDate { get; set; }
    public int Duration { get; set; }
    public string IsCritical { get; set; }

    public List<string> Resources { get; set; }
}