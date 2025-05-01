using Domain.Exceptions;

namespace Domain.Test
{
    [TestClass]
    public class TaskTests
    {
        [TestMethod]
        public void NewTask_WhenConstructorIsNotEmpty_ThenTaskIsNotNull()
        {
            // Arrange
            Task task;
            DateTime startDate = DateTime.Now;
            List<Task> previousTasks = new List<Task>();

            // Act
            task = new Task("Title", "Description", startDate,1, previousTasks);

            // Assert
            Assert.IsNotNull(task);
        }

        [TestMethod]
        [ExpectedException(typeof(TaskTitleException))]
        public void NewTask_WhenTitleIsEmptyOrWhiteSpaces_ThenThrowTaskTitleException()
        {
            // Arrange
            Task task;
            DateTime startDate = DateTime.Now;
            List<Task> previousTasks = new List<Task>();

            // Act
            task = new Task("", "Description", startDate, 1, previousTasks);
        }

        [TestMethod]
        [ExpectedException(typeof(TaskDescriptionException))]
        public void NewTask_WhenDescriptionIsEmptyOrWhiteSpaces_ThenThrowTaskDescriptionException()
        {
            // Arrange
            Task task;
            DateTime startDate = DateTime.Now;
            List<Task> previousTasks = new List<Task>();

            // Act
            task = new Task("Title", "", startDate, 1, previousTasks);
        }

        [TestMethod]
        [ExpectedException(typeof(TaskDurationException))]
        public void NewTask_WhenDurationIsZeroOrNegative_ThenThrowTaskDurationException()
        {
            // Arrange
            Task task;
            DateTime startDate = DateTime.Now;
            List<Task> previousTasks = new List<Task>();

            // Act
            task = new Task("Title", "Description", startDate, -11, previousTasks); 
        }
        
    }
}
