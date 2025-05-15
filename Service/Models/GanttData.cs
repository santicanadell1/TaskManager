namespace Service.Models;

public class GanttData
{
    public List<GanttTask> data { get; set; }
    public List<GanttLink> links { get; set; }
    public List<int> criticalPathIds { get; set; }
}