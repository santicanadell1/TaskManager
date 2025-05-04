using DataAccess;
using Domain;
using Service.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Domain.Exceptions;


namespace Service.Test
{
    [TestClass]
    public class TaskServiceTest
    {
        private InMemoryDatabase _database;
        private TaskService _taskService;

    
        [TestMethod]
        public void CalculateEarlyStart_ShouldReturnCorrectDate_WhenNoPreviousTasks()
        {
            Task task = new Task(
                "Task 1",               
                "Description of Task 1", 
                new DateTime(2025, 5, 1), 
                5,                       
                new List<Task>(),  
                new List<Task>()   
            );

            var earlyStart = _taskService.CalculateEarlyStart(task);

           
            Assert.AreEqual(new DateTime(2025, 5, 1), earlyStart);
        }



    }
}