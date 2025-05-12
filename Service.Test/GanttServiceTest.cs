using Service.Models;


namespace Service.Test;

[TestClass]
public class GanttServiceTestTests
{
        
        [TestMethod]
        public void Convert_ShouldAssignProgressCorrectlyBasedOnState()
        {
            // Arrange
            var tasks = new List<TaskDTO>
            {
                new TaskDTO { Id = 1, Title = "TODO", State = StateDTO.TODO, StartDate = DateTime.Now, EndDate = DateTime.Now, Duration = 1, Slack = TimeSpan.Zero, PreviousTasks = new List<TaskDTO>() },
                new TaskDTO { Id = 2, Title = "DOING", State = StateDTO.DOING, StartDate = DateTime.Now, EndDate = DateTime.Now, Duration = 1, Slack = TimeSpan.Zero, PreviousTasks = new List<TaskDTO>() },
                new TaskDTO { Id = 3, Title = "DONE", State = StateDTO.DONE, StartDate = DateTime.Now, EndDate = DateTime.Now, Duration = 1, Slack = TimeSpan.Zero, PreviousTasks = new List<TaskDTO>() }
            };

            // Act
            var result = GanttService.Convert(tasks, new List<TaskDTO>());

            // Assert
            Assert.AreEqual(0.0, result.data.First(t => t.id == 1).progress);
            Assert.AreEqual(0.5, result.data.First(t => t.id == 2).progress);
            Assert.AreEqual(1.0, result.data.First(t => t.id == 3).progress);
        }
        [TestMethod]
        public void Convert_ShouldHandleCriticalPathAndSlackCorrectly()
        {
            // Arrange
            var task = new TaskDTO
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

            var nonCriticalTask = new TaskDTO
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

            var result = GanttService.Convert(new List<TaskDTO> { task, nonCriticalTask },new List<TaskDTO> { task });

            // Assert
            var ganttTask1 = result.data.First(t => t.id == 1);
            var ganttTask2 = result.data.First(t => t.id == 2);

            Assert.IsTrue(ganttTask1.critical);
            Assert.IsFalse(ganttTask2.critical);
            Assert.AreEqual(2.0, ganttTask2.slack);
            Assert.IsFalse(result.links[0].critical); 
        }
        [TestMethod]
        public void Convert_ShouldCreateCorrectTasksAndLinks_WithCriticalFlag()
        {
            throw new NotImplementedException();
        }
}



