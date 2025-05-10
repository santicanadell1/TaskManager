using Domain.Exceptions;
using Domain;
using Task = Domain.Task;

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
        task = new Task("Title", "Description", startDate, 1, previousTasks, sameTimeTasks, new List<Resource>());
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
        task = new Task("", "Description", startDate, 1, previousTasks, sameTimeTasks, new List<Resource>());
    }

    [TestMethod]
    [ExpectedException(typeof(TaskDescriptionException))]
    public void NewTask_WhenDescriptionIsEmptyOrWhiteSpaces_ThenThrowTaskDescriptionException()
    {
        Task task;
        DateTime startDate = DateTime.Now;
        List<Task> previousTasks = new List<Task>();
        List<Task> sameTimeTasks = new List<Task>();
        task = new Task("Title", "", startDate, 1, previousTasks, sameTimeTasks, new List<Resource>());
    }

    [TestMethod]
    [ExpectedException(typeof(TaskDurationException))]
    public void NewTask_WhenDurationIsZeroOrNegative_ThenThrowTaskDurationException()
    {
        Task task;
        DateTime startDate = DateTime.Now;
        List<Task> previousTasks = new List<Task>();
        List<Task> sameTimeTasks = new List<Task>();
        task = new Task("Title", "Description", startDate, -11, previousTasks, sameTimeTasks, new List<Resource>());
    }

    [TestMethod]
    public void NewTask_WhenAddingNewPreviousTask_ThenTaskIsAdded()
    {
        DateTime startDate = DateTime.Now;
        List<Task> previousTasks = new List<Task>();
        List<Task> sameTimeTasks = new List<Task>();
        Task task1 = new Task("Title", "Description", startDate, 1, previousTasks, sameTimeTasks, new List<Resource>());
        Task task2 = new Task("Title1", "Description1", startDate, 1, previousTasks, sameTimeTasks, new List<Resource>());
        task1.AddPreviousTask(task2);
        Assert.IsTrue(task1.PreviousTasks.Contains(task2));
    }

    [TestMethod]
    public void NewTask_WhenDeletingAPreviousTask_ThenTaskIsDeleted()
    {
        DateTime startDate = DateTime.Now;
        List<Task> previousTasks = new List<Task>();
        List<Task> sameTimeTasks = new List<Task>();
        Task task2 = new Task("Title1", "Description1", startDate, 1, new List<Task>(), sameTimeTasks, new List<Resource>());
        previousTasks.Add(task2);
        Task task1 = new Task("Title", "Description", startDate, 1, previousTasks, sameTimeTasks, new List<Resource>());
        task1.RemovePreviousTask(task2);
        Assert.IsFalse(task1.PreviousTasks.Contains(task2));
    }

    [TestMethod]
    public void NewTask_WhenAddingNewSameTimeTask_ThenTaskIsAdded()
    {
        DateTime startDate = DateTime.Now;
        List<Task> previousTasks = new List<Task>();
        List<Task> sameTimeTasks = new List<Task>();
        Task task1 = new Task("Title", "Description", startDate, 1, previousTasks, sameTimeTasks, new List<Resource>());
        Task task2 = new Task("Title1", "Description1", startDate, 1, previousTasks, sameTimeTasks, new List<Resource>());
        task1.AddSameTimeTask(task2);
        Assert.IsTrue(task1.SameTimeTasks.Contains(task2));
    }

    [TestMethod]
    public void NewTask_WhenDeletingASameTimeTask_ThenTaskIsDeleted()
    {
        DateTime startDate = DateTime.Now;
        List<Task> previousTasks = new List<Task>();
        List<Task> sameTimeTasks = new List<Task>();
        Task task2 = new Task("Title1", "Description1", startDate, 1, new List<Task>(), new List<Task>(), new List<Resource>());
        sameTimeTasks.Add(task2);
        Task task1 = new Task("Title", "Description", startDate, 1, previousTasks, sameTimeTasks, new List<Resource>());
        task1.RemoveSameTimeTask(task2);
        Assert.IsFalse(task1.SameTimeTasks.Contains(task2));
    }

    [TestMethod]
    public void ExpectedStartDate_WhenSet_ThenDateIsAssigned()
    {
        DateTime expectedDate = DateTime.Parse("2026-09-01");
        List<Task> previousTasks = new List<Task>();
        List<Task> sameTimeTasks = new List<Task>();
        Task task = new Task("Title", "Description", DateTime.Now, 1, previousTasks, sameTimeTasks, new List<Resource>());

        task.ExpectedStartDate = expectedDate;

        Assert.AreEqual(expectedDate, task.ExpectedStartDate);
    }

    [TestMethod]
    public void EndDate_WhenSet_ThenDateIsAssigned()
    {
        DateTime endDate = DateTime.Parse("2026-09-01");
        List<Task> previousTasks = new List<Task>();
        List<Task> sameTimeTasks = new List<Task>();
        Task task = new Task("Title", "Description", DateTime.Now, 1, previousTasks, sameTimeTasks, new List<Resource>());

        task.EndDate = endDate;

        Assert.AreEqual(endDate, task.EndDate);
    }

    [TestMethod]
    public void State_WhenSet_ThenStateIsAssigned()
    {
        State state = State.DONE;
        List<Task> previousTasks = new List<Task>();
        List<Task> sameTimeTasks = new List<Task>();
        Task task = new Task("Title", "Description", DateTime.Now, 1, previousTasks, sameTimeTasks, new List<Resource>());

        task.State = state;

        Assert.AreEqual(state, task.State);
    }

    [TestMethod]
    public void Id_WhenSet_ThenIdIsAssigned()
    {
        int expectedId = 123;
        List<Task> previousTasks = new List<Task>();
        List<Task> sameTimeTasks = new List<Task>();
        Task task = new Task("Title", "Description", DateTime.Now, 1, previousTasks, sameTimeTasks, new List<Resource>());

        task.Id = expectedId;

        Assert.AreEqual(expectedId, task.Id);
    }

    [TestMethod]
    public void Id_WhenNotSet_ThenIdIsNull()
    {
        List<Task> previousTasks = new List<Task>();
        List<Task> sameTimeTasks = new List<Task>();
        Task task = new Task("Title", "Description", DateTime.Now, 1, previousTasks, sameTimeTasks, new List<Resource>());

        Assert.IsNull(task.Id);
    }
    
    [TestMethod]
    [ExpectedException(typeof(TaskPreviousTaskException))]
    public void SetPreviousTasks_WhenPreviousTasksIsNull_ShouldThrowTaskPreviousTaskException()
    {
       
        Task task = new Task("Title", "Description", DateTime.Now, 5, new List<Task>(), new List<Task>(), new List<Resource>());

       
        task.PreviousTasks = null;  
    }
    
        [TestMethod]
        [ExpectedException(typeof(TaskResourceException))]
        public void SetResources_WhenResourcesIsNull_ShouldThrowTaskResourceException()
        {
           
            Task task = new Task("Title", "Description", DateTime.Now, 5, new List<Task>(), new List<Task>(), new List<Resource>());

           
            task.Resource = null;  
        }
        
        [TestMethod]
        [ExpectedException(typeof(TaskResourceException))]
        public void AddPreviousTask_WhenTaskIsNull_ShouldThrowTaskResourceException()
        {
            
            Task task = new Task("Title", "Description", DateTime.Now, 5, new List<Task>(), new List<Task>(), new List<Resource>());

           
            task.AddPreviousTask(null); 
        }
    
        [TestMethod]
        public void AddPreviousTask_WhenTaskIsValid_ShouldAddToPreviousTasks()
        {
            
            Task task1 = new Task("Title", "Description", DateTime.Now, 5, new List<Task>(), new List<Task>(), new List<Resource>());
            Task task2 = new Task("Title 2", "Description 2", DateTime.Now, 3, new List<Task>(), new List<Task>(), new List<Resource>());
            
            task1.AddPreviousTask(task2);

            
            Assert.IsTrue(task1.PreviousTasks.Contains(task2));
        }
        
        [TestMethod]
        [ExpectedException(typeof(TaskResourceException))]
        public void RemovePreviousTask_WhenTaskIsNull_ShouldThrowTaskResourceException()
        {
            
            Task task1 = new Task("Title", "Description", DateTime.Now, 5, new List<Task>(), new List<Task>(), new List<Resource>());
    
            
            task1.RemovePreviousTask(null);  
        }
        
        [TestMethod]
        public void RemovePreviousTask_WhenTaskIsValid_ShouldRemoveFromPreviousTasks()
        {
            
            Task task1 = new Task("Title", "Description", DateTime.Now, 5, new List<Task>(), new List<Task>(), new List<Resource>());
            Task task2 = new Task("Title 2", "Description 2", DateTime.Now, 3, new List<Task>(), new List<Task>(), new List<Resource>());
            task1.AddPreviousTask(task2);

          
            task1.RemovePreviousTask(task2);

            
            Assert.IsFalse(task1.PreviousTasks.Contains(task2));
        }
        
        
        [TestMethod]
        [ExpectedException(typeof(TaskResourceException))]
        public void AddSameTimeTask_WhenTaskIsNull_ShouldThrowTaskResourceException()
        {
           
            Task task1 = new Task("Title", "Description", DateTime.Now, 5, new List<Task>(), new List<Task>(), new List<Resource>());

            
            task1.AddSameTimeTask(null);  
        }

        [TestMethod]
        public void AddSameTimeTask_WhenTaskIsValid_ShouldAddToSameTimeTasks()
        {
           
            Task task1 = new Task("Title", "Description", DateTime.Now, 5, new List<Task>(), new List<Task>(), new List<Resource>());
            Task task2 = new Task("Title 2", "Description 2", DateTime.Now, 3, new List<Task>(), new List<Task>(), new List<Resource>());

           
            task1.AddSameTimeTask(task2);

           
            Assert.IsTrue(task1.SameTimeTasks.Contains(task2));
        }
        
        [TestMethod]
        [ExpectedException(typeof(TaskResourceException))]
        public void RemoveSameTimeTask_WhenTaskIsNull_ShouldThrowTaskResourceException()
        {
           
            Task task1 = new Task("Title", "Description", DateTime.Now, 5, new List<Task>(), new List<Task>(), new List<Resource>());
    
           
            task1.RemoveSameTimeTask(null); 
        }
        
        
        [TestMethod]
        public void RemoveSameTimeTask_WhenTaskIsValid_ShouldRemoveFromSameTimeTasks()
        {
           
            Task task1 = new Task("Title", "Description", DateTime.Now, 5, new List<Task>(), new List<Task>(), new List<Resource>());
            Task task2 = new Task("Title 2", "Description 2", DateTime.Now, 3, new List<Task>(), new List<Task>(), new List<Resource>());
            task1.AddSameTimeTask(task2);

           
            task1.RemoveSameTimeTask(task2);

          
            Assert.IsFalse(task1.SameTimeTasks.Contains(task2));
        }
        
        [TestMethod]
        public void LatestStart_WhenNewTaskIsCreated_ShouldBeEqualToStartDate()
        {
            DateTime startDate = DateTime.Now;
            List<Task> previousTasks = new List<Task>();
            List<Task> sameTimeTasks = new List<Task>();
            Task task = new Task("Title", "Description", startDate, 5, previousTasks, sameTimeTasks, new List<Resource>());

            Assert.AreEqual(startDate, task.LatestStart);
        }
        
        [TestMethod]
        public void LatestFinish_WhenNewTaskIsCreated_ShouldBeEqualToEndDate()
        {
            DateTime startDate = DateTime.Now;
            List<Task> previousTasks = new List<Task>();
            List<Task> sameTimeTasks = new List<Task>();
            Task task = new Task("Title", "Description", startDate, 5, previousTasks, sameTimeTasks, new List<Resource>());
        
            DateTime expectedEndDate = startDate.AddDays(5);
            Assert.AreEqual(expectedEndDate, task.LatestFinish);
        }
        
        [TestMethod]
        public void Slack_WhenNewTaskIsCreated_ShouldBeZero()
        {
            DateTime startDate = DateTime.Now;
            List<Task> previousTasks = new List<Task>();
            List<Task> sameTimeTasks = new List<Task>();
            Task task = new Task("Title", "Description", startDate, 5, previousTasks, sameTimeTasks, new List<Resource>());
        
            Assert.AreEqual(TimeSpan.Zero, task.Slack);
        }
        
        [TestMethod]
        public void IsCritical_WhenNewTaskIsCreated_ShouldBeFalse()
        {
            DateTime startDate = DateTime.Now;
            List<Task> previousTasks = new List<Task>();
            List<Task> sameTimeTasks = new List<Task>();
            Task task = new Task("Title", "Description", startDate, 5, previousTasks, sameTimeTasks, new List<Resource>());
        
            Assert.IsFalse(task.IsCritical);
        }
        
        [TestMethod]
        public void SetLatestStart_WhenAssigned_ShouldUpdateValue()
        {
            DateTime startDate = DateTime.Now;
            DateTime newLatestStart = startDate.AddDays(3);
            List<Task> previousTasks = new List<Task>();
            List<Task> sameTimeTasks = new List<Task>();
            Task task = new Task("Title", "Description", startDate, 5, previousTasks, sameTimeTasks, new List<Resource>());

            task.LatestStart = newLatestStart;

            Assert.AreEqual(newLatestStart, task.LatestStart);
        }


}
