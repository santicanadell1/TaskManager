using DataAccess;
using Domain;
using Domain.Exceptions;
using Domain.Test;
using Service.Models;
using Task = Domain.Task;

namespace Service.Test
{
    [TestClass]
    public class TaskServiceTest
    {
        private InMemoryDatabase _database;
        private TaskService _taskService;

        [TestInitialize]
        public void Setup()
        {
            _database = new InMemoryDatabase();
            _taskService = new TaskService(_database);
        }


        [TestMethod]
        public void CalculateEarlyStart_ShouldReturnCorrectDate_WhenNoPreviousTasks()
        {
            var task = new Task(
                "Task 1",
                "Description of Task 1",
                new DateTime(2025, 5, 1),
                5,
                new List<Task>(),
                new List<Task>()
            );

            DateTime earlyStart = _taskService.CalculateEarlyStart(task);


            Assert.AreEqual(new DateTime(2025, 5, 1), earlyStart);
        }

        [TestMethod]
        public void CalculateEarlyFinish_ShouldReturnCorrectDate_WhenGivenDuration()
        {
            var task = new Task(
                "Task 1",
                "Description of Task 1",
                new DateTime(2025, 5, 1),
                5,
                new List<Task>(),
                new List<Task>()
            );

            DateTime earlyFinish = _taskService.CalculateEarlyFinish(task);


            Assert.AreEqual(new DateTime(2025, 5, 6), earlyFinish);
        }

        [TestMethod]
        public void CalculateLateStart_ShouldReturnCorrectDate_WhenThereArePreviousTasks()
        {
            var task1 = new Task("Task 1", "Description of Task 1", new DateTime(2025, 5, 1), 5, new List<Task>(), new List<Task>());
            var task2 = new Task("Task 2", "Description of Task 2", new DateTime(2025, 5, 6), 3, new List<Task> { task1 }, new List<Task>());

            DateTime lateStart = _taskService.CalculateLateStart(task2);

            Assert.AreEqual(new DateTime(2025, 5, 6), lateStart);  
        }

     

        [TestMethod]
        public void IsCritical_ShouldReturnTrue_WhenTaskHasNoFloat()
        {
            var task1 = new Task(
                "Task 1",
                "Description of Task 1",
                new DateTime(2025, 5, 1),
                5,
                new List<Task>(),
                new List<Task>()
            );

            var task2 = new Task(
                "Task 2",
                "Description of Task 2",
                new DateTime(2025, 5, 6),
                3,
                new List<Task> { task1 },
                new List<Task>()
            );


            bool isCritical = _taskService.IsCritical(task2);


            Assert.IsTrue(isCritical);
        }


        [TestMethod]
        public void FromEntity_ShouldConvertTaskToTaskDTO()
        {
            var task = new Task(
                "Task 1",
                "Description of Task 1",
                new DateTime(2025, 5, 1),
                5,
                new List<Task>(),
                new List<Task>()
            );

            TaskDTO taskDTO = _taskService.FromEntity(task);


            Assert.AreEqual(task.Title, taskDTO.Title);
            Assert.AreEqual(task.Description, taskDTO.Description);
            Assert.AreEqual(task.ExpectedStartDate, taskDTO.ExpectedStartDate);
            Assert.AreEqual(task.Duration, taskDTO.Duration);
            Assert.AreEqual(task.State, taskDTO.State);
        }

        [TestMethod]
        public void ToEntity_ShouldConvertTaskDTOToTask()
        {
            var taskDTO = new TaskDTO
            {
                Title = "Task 1",
                Description = "Description of Task 1",
                ExpectedStartDate = new DateTime(2025, 5, 1),
                Duration = 5,
                PreviousTasks = new List<Task>(),
                SameTimeTasks = new List<Task>(),
                State = State.TODO
            };

            Task task = _taskService.ToEntity(taskDTO);


            Assert.AreEqual(taskDTO.Title, task.Title);
            Assert.AreEqual(taskDTO.Description, task.Description);
            Assert.AreEqual(taskDTO.ExpectedStartDate, task.ExpectedStartDate);
            Assert.AreEqual(taskDTO.Duration, task.Duration);
            Assert.AreEqual(taskDTO.State, task.State);
        }


        [TestMethod]
        public void FromEntity_ShouldMapStateCorrectly()
        {
            var task = new Task(
                "Task 1",
                "Description of Task 1",
                new DateTime(2025, 5, 1),
                5,
                new List<Task>(),
                new List<Task>()
            );


            task.State = State.DOING;


            TaskDTO taskDTO = _taskService.FromEntity(task);


            Assert.AreEqual(task.State, taskDTO.State);
        }
        
        
        [TestMethod]
        public void ToEntity_ShouldConvertTaskDTOToTaskCorrectly()
        {
            var taskDTO = new TaskDTO
            {
                Title = "Task 1",
                Description = "Description of Task 1",
                ExpectedStartDate = new DateTime(2025, 5, 1),
                Duration = 5,
                PreviousTasks = new List<Task>(),
                SameTimeTasks = new List<Task>(),
                State = State.TODO
            };

            Task task = _taskService.ToEntity(taskDTO);

            Assert.AreEqual(taskDTO.Title, task.Title);
            Assert.AreEqual(taskDTO.Description, task.Description);
            Assert.AreEqual(taskDTO.ExpectedStartDate, task.ExpectedStartDate);
            Assert.AreEqual(taskDTO.Duration, task.Duration);
            Assert.AreEqual(taskDTO.State, task.State);
        }

        [TestMethod]
        public void FromEntity_ShouldHandleEmptyListsCorrectly()
        {
            var task = new Task("Task 1", "Description of Task 1", new DateTime(2025, 5, 1), 5, new List<Task>(), new List<Task>());
    
            TaskDTO taskDTO = _taskService.FromEntity(task);
    
            Assert.IsNotNull(taskDTO.PreviousTasks);
            Assert.IsNotNull(taskDTO.SameTimeTasks);
        }
        
        [TestMethod]
        public void CalculateEarlyStart_ShouldReturnCorrectDate_WhenThereArePreviousTasks()
        {
            var task1 = new Task("Task 1", "Description of Task 1", new DateTime(2025, 5, 1), 5, new List<Task>(), new List<Task>());
            var task2 = new Task("Task 2", "Description of Task 2", new DateTime(2025, 5, 6), 3, new List<Task> { task1 }, new List<Task>());

            DateTime earlyStart = _taskService.CalculateEarlyStart(task2);

            Assert.AreEqual(new DateTime(2025, 5, 6), earlyStart);  
        }
        
      [TestMethod]
     public void CalculateLateFinish_ShouldReturnEarlyFinish_WhenNoPreviousTasks()
    {
      var task = new Task("Task 1", "Description of Task 1", new DateTime(2025, 5, 1), 5, new List<Task>(), new List<Task>());
  
      DateTime lateFinish = _taskService.CalculateLateFinish(task);
  
      Assert.AreEqual(new DateTime(2025, 5, 6), lateFinish);  
     }
     

    }
}