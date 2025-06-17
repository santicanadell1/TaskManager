using Domain.Exceptions.ProjectExceptions;

namespace Domain.Test;

[TestClass]
public class ProjectTest
{
    [TestMethod]
    public void GivenProject_WhenNameIsSet_ThenNameShouldBeCorrect()
    {
        Project project = new Project();
        String expectedName = "Project A";


        project.Name = expectedName;
        String actualName = project.Name;


        Assert.AreEqual(expectedName, actualName);
    }

    [TestMethod]
    public void GivenProject_WhenDescriptionIsSet_ThenDescriptionShouldBeCorrect()
    {
        Project project = new Project();
        String expectedDescription = "This is a test project";


        project.Description = expectedDescription;
        String actualDescription = project.Description;


        Assert.AreEqual(expectedDescription, actualDescription);
    }

    [TestMethod]
    public void GivenProject_WhenStartDateIsSet_ThenStartDateShouldBeCorrect()
    {
        Project project = new Project();
        DateTime expectedStartDate = DateTime.Today;


        project.StartDate = expectedStartDate;
        DateTime actualStartDate = project.StartDate;


        Assert.AreEqual(expectedStartDate, actualStartDate);
    }

    [TestMethod]
    public void GivenProject_WhenMembersAreSet_ThenMembersShouldBeCorrect()
    {
        Project project = new Project();
        List<User> expectedMembers = new List<User>();


        project.Members = expectedMembers;
        List<User> actualMembers = project.Members;


        CollectionAssert.AreEqual(expectedMembers, actualMembers);
    }

    [TestMethod]
    public void GivenTaskList_WhenTasksAreAdded_ThenTaskListShouldContainCorrectTasks()
    {
        Project project = new Project();
        List<Task> expectedTasks = new List<Task>();


        project.Tasks = expectedTasks;
        List<Task> actualTasks = project.Tasks;


        CollectionAssert.AreEqual(expectedTasks, actualTasks);
    }

    [TestMethod]
    public void GivenProject_WhenAdminProjectIsSet_ThenAdminProjectShouldBeCorrect()
    {
        Project project = new Project();
        User adminUser = new User();
        adminUser.Roles = new List<Rol>();
        adminUser.AddRol(Rol.AdminProject);


        project.AdminProject = adminUser;
        User actualAdminProject = project.AdminProject;


        Assert.IsTrue(actualAdminProject.Roles.Contains(Rol.AdminProject));
    }

    [TestMethod]
    [ExpectedException(typeof(ProjectNameException))]
    public void GivenProject_WhenNameIsNull_ThenProjectNameExceptionShouldBeThrown()
    {
        Project project = new Project();


        project.Name = null;
    }

    [TestMethod]
    [ExpectedException(typeof(ProjectDescriptionException))]
    public void GivenProject_WhenDescriptionIsEmpty_ThenProjectDescriptionExceptionShouldBeThrown()
    {
        Project project = new Project();


        project.Description = "";
    }

    [TestMethod]
    [ExpectedException(typeof(ProjectStartDateException))]
    public void GivenProject_WhenStartDateIsDefault_ThenProjectStartDateExceptionShouldBeThrown()
    {
        Project project = new Project();


        project.StartDate = default;
    }

    [TestMethod]
    public void GivenProject_WhenAddingANewUser_ThenUserShouldBeAddedToProjectMembers()
    {
        Project project = new Project();
        User user1 = new User();
        User user2 = new User();
        project.AddMember(user1);
        project.AddMember(user2);
        Assert.IsTrue(project.Members.Contains(user1));
        Assert.IsTrue(project.Members.Contains(user2));
    }

    [TestMethod]
    public void GivenProject_WhenAddingANewTask_ThenTaskShouldBeAddedToProjectTasks()
    {
        Project project = new Project();
        DateTime startDate = DateTime.Now;
        DateTime endDate = DateTime.Parse("2026-09-01");
        List<Task> previousTasks = new List<Task>();
        Task task1 = new Task("Title", "Description", startDate, 1, previousTasks, null, null);
        project.AddTask(task1);
        Assert.IsTrue(project.Tasks.Contains(task1));
    }

    [TestMethod]
    public void CreateProject_With3parameters_ThenProjectShouldBeCreated()
    {
        Project project = new Project("Project A", "This is a test project", DateTime.Today);
    }

    [TestMethod]
    public void GivenProject_WhenProjectLeaderIsSet_ThenProjectLeaderShouldBeCorrect()
    {
        Project project = new Project();
        User leader = new User { Roles = new List<Rol> { Rol.ProjectLeader } };

        project.SetProjectLeader(leader);
        User actualLeader = project.ProjectLeader;

        Assert.AreEqual(leader, actualLeader);
    }
}