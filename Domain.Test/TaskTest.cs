namespace Domain.Test;
using Domain;

[TestClass]
public class TaskTests
{
    [TestMethod]
    public void NewTask_WhenConstructorIsNotEmpty_ThenTaskIsNotNull()
    {
        //Arrange
        Task task;
        DateTime startDate = DateTime.Now;
        DateTime endDate = DateTime.Today;
        List<Task> previousTasks = new List<Task>();
        //Act
        task = new Task("Title", "Description", startDate, endDate ,1, previousTasks);
        //Assert
        Assert.IsNotNull(task);
    }
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void NewTask_WhenTitleIsEmptyOrWhiteSpaces_ThenThrowArgumentException()
    {
        //Arrange
        Task task;
        DateTime startDate = DateTime.Now;
        DateTime endDate = DateTime.Today;
        List<Task> previousTasks = new List<Task>();
        //Act
        task = new Task("", "Description", startDate, endDate , 1, previousTasks);
    }
}