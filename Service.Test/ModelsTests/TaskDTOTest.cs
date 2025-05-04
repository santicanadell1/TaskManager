using Domain;
using Service.Models;

namespace Service.Test.ModelsTests;

[TestClass]
public class TaskDTOTest
{
    [TestMethod]
    public void NewTask_WhenTitleIsNull_ThenTaskIsNotCreated()
    {
       
        TaskDTO user = new TaskDTO { Title = null };

       
        Assert.IsNull(user.Title);
    }
    
    [TestMethod]
    public void NewTask_WhenDescriptionIsNull_ThenTaskIsNotCreated()
    {
       
        TaskDTO user = new TaskDTO { Description = null };

       
        Assert.IsNull(user.Description);
    }
    
    [TestMethod]
    public void NewTask_WhenExpectedStartDateIsValid_ThenTaskIsCreated()
    {
       
        DateTime validDate = new DateTime(2025, 5, 3); 
        TaskDTO task = new TaskDTO { ExpectedStartDate = validDate };

       
        Assert.AreEqual(validDate, task.ExpectedStartDate);
    }
    
    [TestMethod]
    public void NewTask_WhenDurationIsValid_ThenTaskIsCreated()
    {
      
        int validDuration = 5;
        TaskDTO task = new TaskDTO { Duration = validDuration };

   
        Assert.AreEqual(validDuration, task.Duration);
    }
    [TestMethod]
    public void NewTask_WhenPreviousTasksAreAssigned_ThenPreviousTasksAreSet()
    {
      
        var previousTasks = new List<Domain.Task>
        {
            new Domain.Task("Task 1", "Description 1", DateTime.Now, 3, new List<Domain.Task>(), new List<Domain.Task>()),
            new Domain.Task("Task 2", "Description 2", DateTime.Now, 5, new List<Domain.Task>(), new List<Domain.Task>())
        };
        var taskDTO = new TaskDTO { PreviousTasks = previousTasks };

   
        Assert.IsNotNull(taskDTO.PreviousTasks);
        Assert.AreEqual(2, taskDTO.PreviousTasks.Count);
        Assert.AreEqual("Task 1", taskDTO.PreviousTasks[0].Title);
        Assert.AreEqual("Task 2", taskDTO.PreviousTasks[1].Title);
    }


}