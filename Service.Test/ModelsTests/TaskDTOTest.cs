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
    
    [TestMethod]
    public void NewTask_WhenSameTimeTasksAreAssigned_ThenSameTimeTasksAreSet()
    {
        // arrange
        var sameTimeTasks = new List<Domain.Task>
        {
            new Domain.Task("Task A", "Description A", DateTime.Now, 3, new List<Domain.Task>(), new List<Domain.Task>()),
            new Domain.Task("Task B", "Description B", DateTime.Now, 2, new List<Domain.Task>(), new List<Domain.Task>())
        };
        var taskDto = new TaskDTO { SameTimeTasks = sameTimeTasks };


        Assert.IsNotNull(taskDto.SameTimeTasks);
        Assert.AreEqual(2, taskDto.SameTimeTasks.Count);
        Assert.AreEqual("Task A", taskDto.SameTimeTasks[0].Title);
        Assert.AreEqual("Task B", taskDto.SameTimeTasks[1].Title);
    }
    
    [TestMethod]
    public void NewTask_WhenStateIsSet_ThenStateIsAssignedCorrectly()
    {
     
        var stateTodo = State.TODO;
        var stateDoing = State.DOING;
        var stateDone = State.DONE;

        var taskTodo = new TaskDTO { State = stateTodo };
        var taskDoing = new TaskDTO { State = stateDoing };
        var taskDone = new TaskDTO { State = stateDone };

   
        Assert.AreEqual<State>(stateTodo, taskTodo.State); 
        Assert.AreEqual<State>(stateDoing, taskDoing.State);
        Assert.AreEqual<State>(stateDone, taskDone.State);
    }
  
    [TestMethod]
    public void NewTask_WhenIdIsNotAssigned_ThenIdIsNull()
    {
        TaskDTO task = new TaskDTO(); 

        Assert.IsNull(task.Id); 
    }

    [TestMethod]
    public void NewTask_WhenIdIsAssigned_ThenIdIsSetCorrectly()
    {
        int validId = 123;
        TaskDTO task = new TaskDTO { Id = validId }; 
        
        Assert.AreEqual(validId, task.Id); 
    }

    [TestMethod]
    public void NewTask_WhenIdIsExplicitlyNull_ThenIdIsNull()
    {
        TaskDTO task = new TaskDTO { Id = null }; 

        Assert.IsNull(.Id); 
    }



}