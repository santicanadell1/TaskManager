using DataAccess.ProjectRepositoryExceptions;
using Domain;
using Task = Domain.Task;

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
    [ExpectedException(typeof(DuplicatedProjectsNameException))]
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
    public void AddNewProject_WhenGettingProject_ShouldReturnProject()
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

    [TestMethod]
    public void DeleteProject_WhenGettingProject_ShouldBeNull()
    {
        ProjectRepository projectRepository;
        projectRepository = new ProjectRepository();
        Project project = new Project();
        project.Name = "Project 1";
        Project project2 = new Project();
        project2.Name = "Project 2";
        projectRepository.AddProject(project);
        projectRepository.AddProject(project2);
        projectRepository.RemoveProject(project.Name);
        Assert.IsNull(projectRepository.GetProject(p=> p.Name == "Project 1"));
    }
    [TestMethod]
    [ExpectedException(typeof(ProjectNotFoundException))]
    public void DeleteProject_WhenDeletingAgain_ShouldThrowProjectNotFoundException()
    {
        ProjectRepository projectRepository;
        projectRepository = new ProjectRepository();
        Project project = new Project();
        project.Name = "Project 1";
        Project project2 = new Project();
        project2.Name = "Project 2";
        projectRepository.AddProject(project);
        projectRepository.AddProject(project2);
        projectRepository.RemoveProject(project.Name);
        projectRepository.RemoveProject(project.Name);
    }
    
    [TestMethod]
    public void UpdateProject_WhenGettingProject_ShouldBeDifferentFromTheOriginalProject()
    {
        ProjectRepository projectRepository;
        projectRepository = new ProjectRepository();
        Project project = new Project();
        project.Name = "Project 1";
        project.Description = "Project 1 description";
        Project project2 = new Project();
        project2.Name = "Project 1";
        project2.Description = "Project 2 description";
        projectRepository.AddProject(project);
        projectRepository.UpdateProject(project.Name,project2);
        Assert.AreNotEqual(projectRepository.GetProject(p=> p.Name == "Project 1"), project);
    }
    
    [TestMethod]
    [ExpectedException(typeof(DuplicatedProjectsNameException))]
    public void updateProject_WhenNameIsDuplicated_ShouldThrowDuplicatedNameException()
    {
        ProjectRepository projectRepository;
        projectRepository = new ProjectRepository();
        Project project = new Project();
        project.Name = "Project 1";
        project.Description = "Project 1 description";
        Project project2 = new Project();
        project2.Name = "Project 2";
        project2.Description = "Project 2 description";
        Project project3 = new Project();
        project3.Name = "Project 2";
        project3.Description = "Project 3 description";
        projectRepository.AddProject(project);
        projectRepository.AddProject(project2);
        projectRepository.UpdateProject(project.Name,project3);
    }
    
    [TestMethod]
    [ExpectedException(typeof(ProjectNotFoundException))]
    public void updateProject_WhenNameIsNotFound_ShouldThrowProjectNotFoundException()
    {
        ProjectRepository projectRepository;
        projectRepository = new ProjectRepository();
        Project project = new Project();
        project.Name = "Project 1";
        project.Description = "Project 1 description";
        Project project2 = new Project();
        project2.Name = "Project 2";
        project2.Description = "Project 2 description";
        Project project3 = new Project();
        projectRepository.AddProject(project);
        projectRepository.UpdateProject("Project 4",project2);
    }
    [TestMethod]
    public void AddTask_WhenAddingNewTask_ShouldContainIt()
    {
        ProjectRepository projectRepository = new ProjectRepository();
        Project project = new Project();
        project.Name = "Project 1";
    
        
        Task task = new Task(
            "Task 1",                          
            "Task 1 description",             
            DateTime.Now,                      
            5,                                  
            new List<Task>(),                  
            new List<Task>()                    
        );

        projectRepository.AddProject(project);
        projectRepository.AddTask(project.Name, task);

        Assert.IsTrue(project.Tasks.Contains(task));
    }
}