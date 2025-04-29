using Domain.Exceptions;

namespace Domain.Test
{
    [TestClass]
    public class TaskTests
    {
        [TestMethod]
        public void NewTask_WhenConstructorIsNotEmpty_ThenTaskIsNotNull()
        {
            
            Task task;
            DateTime startDate = DateTime.Now;
            DateTime endDate = DateTime.Parse("2026-09-01");
            List<Task> previousTasks = new List<Task>();

            
            task = new Task("Title", "Description", startDate, endDate, 1, previousTasks);

            
            Assert.IsNotNull(task);
        }

        [TestMethod]
        [ExpectedException(typeof(TaskTitleException))]
        public void NewTask_WhenTitleIsEmptyOrWhiteSpaces_ThenThrowTaskTitleException()
        {
         
            Task task;
            DateTime startDate = DateTime.Now;
            DateTime endDate = DateTime.Today;
            List<Task> previousTasks = new List<Task>();

            
            task = new Task("", "Description", startDate, endDate, 1, previousTasks);
        }

        [TestMethod]
        [ExpectedException(typeof(TaskDescriptionException))]
        public void NewTask_WhenDescriptionIsEmptyOrWhiteSpaces_ThenThrowTaskDescriptionException()
        {
         
            Task task;
            DateTime startDate = DateTime.Now;
            DateTime endDate = DateTime.Today;
            List<Task> previousTasks = new List<Task>();

           
            task = new Task("Title", "", startDate, endDate, 1, previousTasks);
        }

        [TestMethod]
        [ExpectedException(typeof(TaskDurationException))]
        public void NewTask_WhenDurationIsZeroOrNegative_ThenThrowTaskDurationException()
        {
           
            Task task;
            DateTime startDate = DateTime.Now;
            DateTime endDate = DateTime.Parse("2026-09-01");
            List<Task> previousTasks = new List<Task>();

          
            task = new Task("Title", "Description", startDate, endDate, -11, previousTasks); // Should throw exception
        }

        [TestMethod]
        [ExpectedException(typeof(TaskEndDateException))]
        public void NewTask_WhenExpectedEndDateIsBeforeExpectedStartDate_ThenThrowTaskEndDateException()
        {
         
            Task task;
            DateTime startDate = DateTime.Parse("2023-01-01");
            DateTime endDate = DateTime.Parse("2022-01-01");
            List<Task> previousTasks = new List<Task>();

            
            task = new Task("Title", "Description", startDate, endDate, 1, previousTasks); // Should throw exception
        }
    }
}
