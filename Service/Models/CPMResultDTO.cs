namespace Service.Models;

public class CpmResultDTO
{
    public int ProjectDuration { get; set; }

    public List<int?> CriticalPathIds { get; set; }

    public List<int?> CriticalTaskIds { get; set; }

    public DateTime EarliestStartDate { get; set; }

    public DateTime LatestFinishDate { get; set; }
}