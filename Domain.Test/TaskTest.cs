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
            List<Task> sameTimeTasks = new List<Task>();
            task = new Task("Title", "Description", startDate,1, previousTasks, sameTimeTasks);
            Assert.IsNotNull(task);
        }

        [TestMethod]
        [ExpectedException(typeof(TaskTitleException))]
        public void NewTask_WhenTitleIsEmptyOrWhiteSpaces_ThenThrowTaskTitleException()
        {
            Task task;
            DateTime startDate = DateTime.Now;
            List<Task> previousTasks = new List<Task>();
            List<Task> sameTimeTasks = new List<Task>();
            task = new Task("", "Description", startDate, 1, previousTasks, sameTimeTasks);
        }

        [TestMethod]
        [ExpectedException(typeof(TaskDescriptionException))]
        public void NewTask_WhenDescriptionIsEmptyOrWhiteSpaces_ThenThrowTaskDescriptionException()
        {
            Task task;
            DateTime startDate = DateTime.Now;
            List<Task> previousTasks = new List<Task>();
            List<Task> sameTimeTasks = new List<Task>();
            task = new Task("Title", "", startDate, 1, previousTasks, sameTimeTasks);
        }

        [TestMethod]
        [ExpectedException(typeof(TaskDurationException))]
        public void NewTask_WhenDurationIsZeroOrNegative_ThenThrowTaskDurationException()
        {
            Task task;
            DateTime startDate = DateTime.Now;
            List<Task> previousTasks = new List<Task>();
            List<Task> sameTimeTasks = new List<Task>();
            task = new Task("Title", "Description", startDate, -11, previousTasks, sameTimeTasks); 
        }
        [TestMethod]
        public void NewTask_WhenAddingNewPreviousTask_ThenTaskIsAdded()
        {
            DateTime startDate = DateTime.Now;
            List<Task> previousTasks = new List<Task>();
            List<Task> sameTimeTasks = new List<Task>();
            Task task1 = new Task("Title", "Description", startDate,1, previousTasks, sameTimeTasks);
            Task task2 = new Task("Title1", "Description1", startDate,1, previousTasks, sameTimeTasks);
            task1.AddPreviousTask(task2);
            Assert.IsTrue(task1.PreviousTasks.Contains(task2));
        } 
        [TestMethod]
        public void NewTask_WhenDeletingAPreviousTask_ThenTaskIsDeleted()
        {
            DateTime startDate = DateTime.Now;
            List<Task> previousTasks = new List<Task>();
            List<Task> sameTimeTasks = new List<Task>();
            Task task2 = new Task("Title1", "Description1", startDate,1, new List<Task>(), sameTimeTasks);
            previousTasks.Add(task2);
            Task task1 = new Task("Title", "Description", startDate,1, previousTasks, sameTimeTasks);
            task1.RemovePreviousTask(task2);
            Assert.IsFalse(task1.PreviousTasks.Contains(task2));
        }
        [TestMethod]
        public void NewTask_WhenAddingNewSameTimeTask_ThenTaskIsAdded()
        {
            DateTime startDate = DateTime.Now;
            List<Task> previousTasks = new List<Task>();
            List<Task> sameTimeTasks = new List<Task>();
            Task task1 = new Task("Title", "Description", startDate,1, previousTasks, sameTimeTasks);
            Task task2 = new Task("Title1", "Description1", startDate,1, previousTasks, sameTimeTasks);
            task1.AddSameTimeTask(task2);
            Assert.IsTrue(task1.SameTimeTasks.Contains(task2));
        } 
        [TestMethod]
        public void NewTask_WhenDeletingASameTimeTask_ThenTaskIsDeleted()
        {
            DateTime startDate = DateTime.Now;
            List<Task> previousTasks = new List<Task>();
            List<Task> sameTimeTasks = new List<Task>();
            Task task2 = new Task("Title1", "Description1", startDate,1, new List<Task>(), new List<Task>());
            sameTimeTasks.Add(task2);
            Task task1 = new Task("Title", "Description", startDate,1, previousTasks, sameTimeTasks);
            task1.RemoveSameTimeTask(task2);
            Assert.IsFalse(task1.SameTimeTasks.Contains(task2));
        }
        
        [TestMethod]
        public void ExpectedStartDate_WhenSet_ThenDateIsAssigned()
        {
            DateTime expectedDate = DateTime.Parse("2026-09-01");
            List<Task> previousTasks = new List<Task>();
            List<Task> sameTimeTasks = new List<Task>();
            Task task = new Task("Title", "Description", DateTime.Now, 1, previousTasks, sameTimeTasks);
    
           
            task.ExpectedStartDate = expectedDate;

            Assert.AreEqual(expectedDate, task.ExpectedStartDate);
        }

        [TestMethod]
        public void EndDate_WhenSet_ThenDateIsAssigned()
        {
            DateTime endDate = DateTime.Parse("2026-09-01");
            List<Task> previousTasks = new List<Task>();
            List<Task> sameTimeTasks = new List<Task>();
            Task task = new Task("Title", "Description", DateTime.Now, 1, previousTasks, sameTimeTasks);
    
            
            task.EndDate = endDate;

            Assert.AreEqual(endDate, task.EndDate);
        }

        [TestMethod]
        public void State_WhenSet_ThenStateIsAssigned()
        {
            State state = State.DONE; 
            List<Task> previousTasks = new List<Task>();
            List<Task> sameTimeTasks = new List<Task>();
            Task task = new Task("Title", "Description", DateTime.Now, 1, previousTasks, sameTimeTasks);
    
           
            task.State = state;

            Assert.AreEqual(state, task.State);
        }






    }
}
