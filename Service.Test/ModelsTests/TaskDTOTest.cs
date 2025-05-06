using Domain;
using Service.Models;

namespace Service.Test.ModelsTests;

[TestClass]
public class TaskDTOTest
{
    [TestMethod]
    public void NewTask_WhenTitleIsNull_ThenTaskIsNotCreated()
    {
        TaskDTO taskDTO = new TaskDTO { Title = null };
        Assert.IsNull(taskDTO.Title);
    }
    
    [TestMethod]
    public void NewTask_WhenDescriptionIsNull_ThenTaskIsNotCreated()
    {
        TaskDTO taskDTO = new TaskDTO { Description = null };
        Assert.IsNull(taskDTO.Description);
    }
    
    [TestMethod]
    public void NewTask_WhenExpectedStartDateIsValid_ThenTaskIsCreated()
    {
        DateTime validDate = new DateTime(2025, 5, 3); 
        TaskDTO taskDTO = new TaskDTO { ExpectedStartDate = validDate };
        Assert.AreEqual(validDate, taskDTO.ExpectedStartDate);
    }
    
    [TestMethod]
    public void NewTask_WhenDurationIsValid_ThenTaskIsCreated()
    {
        int validDuration = 5;
        TaskDTO taskDTO = new TaskDTO { Duration = validDuration };
        Assert.AreEqual(validDuration, taskDTO.Duration);
    }

    [TestMethod]
    public void NewTask_WhenPreviousTasksAreAssigned_ThenPreviousTasksAreSet()
    {
        var previousTasks = new List<Domain.Task>
        {
            new Domain.Task("Task 1", "Description 1", DateTime.Now, 3, new List<Domain.Task>(), new List<Domain.Task>(), new List<Resource>()),
            new Domain.Task("Task 2", "Description 2", DateTime.Now, 5, new List<Domain.Task>(), new List<Domain.Task>(), new List<Resource>())
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
        var sameTimeTasks = new List<Domain.Task>
        {
            new Domain.Task("Task A", "Description A", DateTime.Now, 3, new List<Domain.Task>(), new List<Domain.Task>(), new List<Resource>()),
            new Domain.Task("Task B", "Description B", DateTime.Now, 2, new List<Domain.Task>(), new List<Domain.Task>(), new List<Resource>())
        };
        var taskDTO = new TaskDTO { SameTimeTasks = sameTimeTasks };
        Assert.IsNotNull(taskDTO.SameTimeTasks);
        Assert.AreEqual(2, taskDTO.SameTimeTasks.Count);
        Assert.AreEqual("Task A", taskDTO.SameTimeTasks[0].Title);
        Assert.AreEqual("Task B", taskDTO.SameTimeTasks[1].Title);
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
    public void NewTask_WhenIdIsSet_ThenIdIsAssigned()
    {
        TaskDTO taskDTO = new TaskDTO { Id = 123 };
        Assert.AreEqual(123, taskDTO.Id);
    }

    [TestMethod]
    public void NewTask_WhenIdIsNotSet_ThenIdIsNull()
    {
        TaskDTO taskDTO = new TaskDTO();
        Assert.IsNull(taskDTO.Id);
    }
    
    [TestMethod]
    public void NewTask_WhenResourcesAreAssigned_ThenResourcesAreSet()
    {
        var resources = new List<Resource>
        {
            new Resource("Resource 1", "Type 1", "Description of Resource 1"),
            new Resource("Resource 2", "Type 2", "Description of Resource 2")
        };
    
        var taskDTO = new TaskDTO { Resources = resources };

        Assert.IsNotNull(taskDTO.Resources);
        Assert.AreEqual(2, taskDTO.Resources.Count);
        Assert.AreEqual("Resource 1", taskDTO.Resources[0].Name);
        Assert.AreEqual("Resource 2", taskDTO.Resources[1].Name);
    }

   

    
}
