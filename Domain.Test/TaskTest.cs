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
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void NewTask_WhenDescriptionIsEmptyOrWhiteSpaces_ThenThrowArgumentException()
    {
        //Arrange
        Task task;
        DateTime startDate = DateTime.Now;
        DateTime endDate = DateTime.Today;
        List<Task> previousTasks = new List<Task>();
        //Act
        task = new Task("Title", "", startDate,  endDate ,1, previousTasks);
    }
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void NewTask_WhenDurationIsZeroOrNegative_ThenThrowArgumentException()
    {
        //Arrange
        Task task;
        DateTime startDate = DateTime.Now;
        DateTime endDate = DateTime.Today;
        List<Task> previousTasks = new List<Task>();
        //Act
        task = new Task("Title", "Description", startDate,  endDate ,-11, previousTasks);
    }
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void NewTask_WhenExpectedEndDateIsBeforeExpectedStartDate_ThenThrowArgumentException()
    {
        //Arrange
        Task task;
        DateTime startDate = DateTime.Parse("2023-01-01");
        DateTime endDate = DateTime.Parse("2022-01-01");
        List<Task> previousTasks = new List<Task>();
        //Act
        task = new Task("Title", "Description", startDate,  endDate ,1, previousTasks);
    }
}