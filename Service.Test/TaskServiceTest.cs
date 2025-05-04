using DataAccess;
using Domain;
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
            var task = new Domain.Task(
                "Task 1",                             
                "Description of Task 1",              
                new DateTime(2025, 5, 1),             
                5,                                     
                new List<Domain.Task>(),              
                new List<Domain.Task>()             
            );

            DateTime earlyFinish = _taskService.CalculateEarlyFinish(task);

       
            Assert.AreEqual(new DateTime(2025, 5, 6), earlyFinish); 
        }
        
        [TestMethod]
        public void CalculateLateStart_ShouldReturnCorrectDate_WhenThereArePreviousTasks()
        {
            var task1 = new Domain.Task(
                "Task 1",                             
                "Description of Task 1",              
                new DateTime(2025, 5, 1),             
                5,                                     
                new List<Domain.Task>(),             
                new List<Domain.Task>()              
            );

            var task2 = new Domain.Task(
                "Task 2",                            
                "Description of Task 2",              
                new DateTime(2025, 5, 6),             
                3,                                     
                new List<Domain.Task> { task1 },     
                new List<Domain.Task>()               
            );

            DateTime lateStart = _taskService.CalculateLateStart(task2);

           
            Assert.AreEqual(new DateTime(2025, 5, 6), lateStart);
        }


   



    }
}