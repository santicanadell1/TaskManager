using Service.Models;

namespace Service.Test;

[TestClass]
public class GanttServiceTestTests
{
    [TestMethod]
    public void Convert_WhenCreatingGanttTask_ShouldAssignProgressCorrectlyBasedOnState()
    {
        List<TaskDTO> tasks = new List<TaskDTO>
        {
            new()
            {
                Id = 1, Title = "TODO", State = StateDTO.TODO, StartDate = DateTime.Now, EndDate = DateTime.Now,
                Duration = 1, Slack = TimeSpan.Zero, PreviousTasks = new List<TaskDTO>()
            },
            new()
            {
                Id = 2, Title = "DOING", State = StateDTO.DOING, StartDate = DateTime.Now, EndDate = DateTime.Now,
                Duration = 1, Slack = TimeSpan.Zero, PreviousTasks = new List<TaskDTO>()
            },
            new()
            {
                Id = 3, Title = "DONE", State = StateDTO.DONE, StartDate = DateTime.Now, EndDate = DateTime.Now,
                Duration = 1, Slack = TimeSpan.Zero, PreviousTasks = new List<TaskDTO>()
            }
        };
        GanttData result = GanttService.Convert(tasks, new List<TaskDTO>());
        Assert.AreEqual(0.0, result.data.First(t => t.id == 1).progress);
        Assert.AreEqual(0.5, result.data.First(t => t.id == 2).progress);
        Assert.AreEqual(1.0, result.data.First(t => t.id == 3).progress);
    }

    [TestMethod]
    public void Convert_WhenCratingGanttTask_ShouldHandleCriticalPathAndSlackCorrectly()
    {
        TaskDTO task = new TaskDTO
        {
            Id = 1,
            Title = "Crítica",
            State = StateDTO.DOING,
            StartDate = new DateTime(2025, 5, 1),
            EndDate = new DateTime(2025, 5, 3),
            Duration = 2,
            Slack = TimeSpan.Zero,
            PreviousTasks = new List<TaskDTO>()
        };
        TaskDTO nonCriticalTask = new TaskDTO
        {
            Id = 2,
            Title = "Opcional",
            State = StateDTO.TODO,
            StartDate = new DateTime(2025, 5, 3),
            EndDate = new DateTime(2025, 5, 5),
            Duration = 2,
            Slack = TimeSpan.FromDays(2),
            PreviousTasks = new List<TaskDTO> { task }
        };
        GanttData result = GanttService.Convert(new List<TaskDTO> { task, nonCriticalTask }, new List<TaskDTO> { task });
        GanttTask ganttTask1 = result.data.First(t => t.id == 1);
        GanttTask ganttTask2 = result.data.First(t => t.id == 2);
        Assert.IsTrue(ganttTask1.critical);
        Assert.IsFalse(ganttTask2.critical);
        Assert.AreEqual(2.0, ganttTask2.slack);
        Assert.IsFalse(result.links[0].critical);
    }

    [TestMethod]
    public void Convert_ShouldCreateCorrectTasksAndLinks_WithCriticalFlag()
    {
        TaskDTO task1 = new TaskDTO
        {
            Id = 1,
            Title = "Inicio",
            StartDate = new DateTime(2025, 5, 1),
            EndDate = new DateTime(2025, 5, 2),
            Duration = 1,
            State = StateDTO.TODO,
            Slack = TimeSpan.Zero,
            PreviousTasks = new List<TaskDTO>()
        };
        TaskDTO task2 = new TaskDTO
        {
            Id = 2,
            Title = "Diseño",
            StartDate = new DateTime(2025, 5, 2),
            EndDate = new DateTime(2025, 5, 4),
            Duration = 2,
            State = StateDTO.DONE,
            Slack = TimeSpan.FromDays(1),
            PreviousTasks = new List<TaskDTO> { task1 }
        };
        List<TaskDTO> allTasks = new List<TaskDTO> { task1, task2 };
        List<TaskDTO> criticalPath = new List<TaskDTO> { task1, task2 };
        GanttData result = GanttService.Convert(allTasks, criticalPath);
        Assert.AreEqual(2, result.data.Count);
        Assert.AreEqual(1, result.links.Count);
        GanttTask ganttTask1 = result.data.First(t => t.id == 1);
        GanttTask ganttTask2 = result.data.First(t => t.id == 2);
        Assert.AreEqual("Inicio", ganttTask1.text);
        Assert.AreEqual("2025-05-01", ganttTask1.start_date);
        Assert.AreEqual("2025-05-02", ganttTask1.end_date);
        Assert.AreEqual(1.0, ganttTask2.progress, 0.01);
        Assert.IsTrue(ganttTask2.critical);
        Assert.AreEqual(1, ganttTask2.slack);
        GanttLink link = result.links.First();
        Assert.AreEqual(1, link.source);
        Assert.AreEqual(2, link.target);
        Assert.AreEqual("0", link.type);
        Assert.IsTrue(link.critical);
        CollectionAssert.AreEqual(new List<int> { 1, 2 }, result.criticalPathIds);
    }
}