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
    public void NewTask_WhenStartDateIsValid_ThenTaskIsCreated()
    {
       
        DateTime validStartDate = new DateTime(2025, 5, 4);
        TaskDTO task = new TaskDTO { StartDate = validStartDate };


        Assert.AreEqual(validStartDate, task.StartDate);
    }


}