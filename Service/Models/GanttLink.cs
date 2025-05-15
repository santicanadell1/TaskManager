namespace Service.Models;

public class GanttLink
{
    public int id { get; set; }
    public int source { get; set; }
    public int target { get; set; }
    public string type { get; set; } = "0";
    public bool critical { get; set; } = false;
}