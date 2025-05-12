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
            throw new NotImplementedException();
        }
}



