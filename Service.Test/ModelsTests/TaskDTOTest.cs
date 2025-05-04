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

}