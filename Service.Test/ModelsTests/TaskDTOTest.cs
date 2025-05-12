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
        var previousTasks = new List<TaskDTO>
        {
            new TaskDTO
            {
                Title = "Task 1", Description = "Description 1", ExpectedStartDate = DateTime.Now, Duration = 3
            },
            new TaskDTO
            {
                Title = "Task 2", Description = "Description 2", ExpectedStartDate = DateTime.Now, Duration = 5
            }
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
        var sameTimeTasks = new List<TaskDTO>
        {
            new TaskDTO
            {
                Title = "Task A", Description = "Description A", ExpectedStartDate = DateTime.Now, Duration = 3
            },
            new TaskDTO
            {
                Title = "Task B", Description = "Description B", ExpectedStartDate = DateTime.Now, Duration = 2
            }
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
        var stateTodo = StateDTO.TODO;
        var stateDoing = StateDTO.DOING;
        var stateDone = StateDTO.DONE;

        var taskTodo = new TaskDTO { State = stateTodo };
        var taskDoing = new TaskDTO { State = stateDoing };
        var taskDone = new TaskDTO { State = stateDone };

        Assert.AreEqual<StateDTO>(stateTodo, taskTodo.State);
        Assert.AreEqual<StateDTO>(stateDoing, taskDoing.State);
        Assert.AreEqual<StateDTO>(stateDone, taskDone.State);
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
        var resources = new List<ResourceDTO>
        {
            new ResourceDTO { Name = "Resource 1", Type = "Type 1", Description = "Description of Resource 1" },
            new ResourceDTO { Name = "Resource 2", Type = "Type 2", Description = "Description of Resource 2" }
        };

        var taskDTO = new TaskDTO { Resources = resources };

        Assert.IsNotNull(taskDTO.Resources);
        Assert.AreEqual(2, taskDTO.Resources.Count);
        Assert.AreEqual("Resource 1", taskDTO.Resources[0].Name);
        Assert.AreEqual("Resource 2", taskDTO.Resources[1].Name);
    }
    
    [TestMethod]
    public void TaskDTO_WhenIsCriticalIsSet_ThenIsCriticalIsAssigned()
    {
        bool isCritical = true;
        TaskDTO taskDTO = new TaskDTO();
            
        taskDTO.IsCritical = isCritical;
            
        Assert.AreEqual(isCritical, taskDTO.IsCritical);
    }
    
    [TestMethod]
    public void TaskDTO_WhenStartDateIsSet_ThenStartDateIsAssigned()
    {
        DateTime startDate = new DateTime(2025, 6, 15);
        TaskDTO taskDTO = new TaskDTO();
            
        taskDTO.StartDate = startDate;
            
        Assert.AreEqual(startDate, taskDTO.StartDate);
    }
    
    [TestMethod]
    public void TaskDTO_WhenEndDateIsSet_ThenEndDateIsAssigned()
    {
        DateTime endDate = new DateTime(2025, 6, 20);
        TaskDTO taskDTO = new TaskDTO();
            
        taskDTO.EndDate = endDate;
            
        Assert.AreEqual(endDate, taskDTO.EndDate);
    }

    
    

}