using Domain;
namespace DataAccess.Test;

[TestClass]
public class ProjectRepositoryTest
{
    [TestMethod]
    public void NewProjectRepository_WhenRepositoryIsCreated_ShouldNotBeNull()
    {
        ProjectRepository projectRepository;
        projectRepository = new ProjectRepository();
        Assert.IsNotNull(projectRepository);
    }
    
    [TestMethod]
    public void NewProject_WhenAddingNewProject_ListShouldContainIt()
    {
        ProjectRepository projectRepository;
        projectRepository = new ProjectRepository();
        Project project = new Project();
        project.Name = "Project 1";
        projectRepository.AddProject(project);
        Assert.IsTrue(projectRepository.GetAllProjects().Contains(project));
    }

    [TestMethod]
    [ExpectedException(DuplicatedNameException)]
    public void AddNewProject_WhenAddingDuplicatedProject_ShouldThrowDuplicatedNameException()
    {
        ProjectRepository projectRepository;
        projectRepository = new ProjectRepository();
        Project project = new Project();
        project.Name = "Project 1";
        projectRepository.AddProject(project);
        projectRepository.AddProject(project);
    }
    
    [TestMethod]
    public void AddNewProject_WhenGettingUser_ShouldReturnUser()
    {
        ProjectRepository projectRepository;
        projectRepository = new ProjectRepository();
        Project project = new Project();
        project.Name = "Project 1";
        Project project2 = new Project();
        project2.Name = "Project 2";
        projectRepository.AddProject(project);
        projectRepository.AddProject(project2);
        Assert.AreEqual(projectRepository.GetProject(u=> u.Name == "Project 2"), project2);

    }
}