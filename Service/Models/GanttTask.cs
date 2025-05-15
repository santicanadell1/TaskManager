namespace Service.Models;

public class GanttTask
{
    public int id { get; set; }
    public string text { get; set; }
    public string start_date { get; set; }
    public string end_date { get; set; }
    public int duration { get; set; }
    public bool critical { get; set; }
    public double slack { get; set; }
    public double progress { get; set; }
}