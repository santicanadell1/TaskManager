using DataAccess.Exceptions.ProjectRepositoryExceptions;
using DataAccess.Exceptions.TaskRepositoryExceptions;
using Domain;
using Task = Domain.Task;

namespace DataAccess.Test;

[TestClass]
public class ProjectRepositoryTest
{
    private AppDbContext _context;
    private ProjectRepository _projectRepository;

    [TestInitialize]
    public void Setup()
    {
        var contextFactory = new InMemoryAppContextFactory();
        _context = contextFactory.CreateDbContext();
        _projectRepository = new ProjectRepository(_context);
    }

    [TestCleanup]
    public void CleanUp()
    {
        _context.Database.EnsureDeleted();
    }

    [TestMethod]
    public void NewProjectRepository_WhenRepositoryIsCreated_ShouldNotBeNull()
    {
        Assert.IsNotNull(_projectRepository);
    }

    [TestMethod]
    public void NewProject_WhenAddingNewProject_ListShouldContainIt()
    {
        var project = new Project { Name = "Project 1", Description = "Project 1 description" }; 
        _projectRepository.AddProject(project);
        _context.SaveChanges();
        Assert.IsTrue(_projectRepository.GetAllProjects().Contains(project));
    }


    [TestMethod]
    [ExpectedException(typeof(DuplicatedProjectsNameException))]
    public void AddNewProject_WhenAddingDuplicatedProject_ShouldThrowDuplicatedNameException()
    {
        var project = new Project { Name = "Project 1", Description = "Project 1 description" };
        _projectRepository.AddProject(project);
        _context.SaveChanges();
    
        var duplicateProject = new Project { Name = "Project 1", Description = "Project 1 description" };
        _projectRepository.AddProject(duplicateProject);
        _context.SaveChanges();  
    }


    [TestMethod]
    public void AddNewProject_WhenGettingProject_ShouldReturnProject()
    {
        var project = new Project { Name = "Project 1", Description = "Project 1 description" };
        var project2 = new Project { Name = "Project 2" , Description = "Project 2 description"};
        _projectRepository.AddProject(project);
        _projectRepository.AddProject(project2);
        _context.SaveChanges();
        Assert.AreEqual(_projectRepository.GetProject(u => u.Name == "Project 2"), project2);
    }

    [TestMethod]
    public void DeleteProject_WhenGettingProject_ShouldBeNull()
    {
        var project = new Project { Name = "Project 1",Description = "Project 1 description"  };
        var project2 = new Project { Name = "Project 2" , Description = "Project 2 description"};
        _projectRepository.AddProject(project);
        _projectRepository.AddProject(project2);
        _context.SaveChanges();
        _projectRepository.RemoveProject(project.Name);
        _context.SaveChanges();
        Assert.IsNull(_projectRepository.GetProject(p => p.Name == "Project 1"));
    }

    [TestMethod]
    [ExpectedException(typeof(ProjectNotFoundException))]
    public void DeleteProject_WhenDeletingAgain_ShouldThrowProjectNotFoundException()
    {
        var project = new Project { Name = "Project 1", Description = "Project 1 description" };
        var project2 = new Project { Name = "Project 2" , Description = "Project 2 description"};
        _projectRepository.AddProject(project);
        _projectRepository.AddProject(project2);
        _context.SaveChanges();
        _projectRepository.RemoveProject(project.Name);
        _context.SaveChanges();
        _projectRepository.RemoveProject(project.Name);
        _context.SaveChanges();
    }
    
    [TestMethod]
    public void UpdateProject_WhenGettingProject_ShouldBeDifferentFromTheOriginalProject()
    {
        var project = new Project { Name = "Project 1", Description = "Project 1 description" };
        var project2 = new Project { Name = "Project 1", Description = "Updated Project description" };  
        _projectRepository.AddProject(project);
        _context.SaveChanges();
    
        _projectRepository.UpdateProject(project.Name, project2);
        _context.SaveChanges();
    
        var updatedProject = _projectRepository.GetProject(p => p.Name == "Project 1");
    
        Assert.AreNotEqual(project.Description, updatedProject.Description);  
        Assert.AreEqual(project.Name, updatedProject.Name);  
    }
    
    [TestMethod]
    [ExpectedException(typeof(DuplicatedProjectsNameException))]
    public void updateProject_WhenNameIsDuplicated_ShouldThrowDuplicatedNameException()
    {
        var project = new Project { Name = "Project 1", Description = "Project 1 description" };
        var project2 = new Project { Name = "Project 2", Description = "Project 2 description" };
        var project3 = new Project { Name = "Project 2", Description = "Project 3 description" };
        _projectRepository.AddProject(project);
        _projectRepository.AddProject(project2);
        _context.SaveChanges();
        _projectRepository.UpdateProject(project.Name, project3);
        _context.SaveChanges();
    }

    [TestMethod]
    [ExpectedException(typeof(ProjectNotFoundException))]
    public void updateProject_WhenNameIsNotFound_ShouldThrowProjectNotFoundException()
    {
        var project = new Project { Name = "Project 1", Description = "Project 1 description" };
        var project2 = new Project { Name = "Project 2", Description = "Project 2 description" };
        _projectRepository.AddProject(project);
        _context.SaveChanges();
        _projectRepository.UpdateProject("Project 4", project2);
        _context.SaveChanges();
    }

    [TestMethod]
    public void AddTask_WhenAddingNewTask_ShouldContainIt()
    {
        var project = new Project { Name = "Project 1" , Description = "Project 1 Description"};
        var task = new Task("Task 1", "Task 1 description", DateTime.Now, 5, new List<Task>(), new List<Task>(), new List<Resource>());
        _projectRepository.AddProject(project);
        _context.SaveChanges();
        _projectRepository.AddTask(project.Name, task);
        _context.SaveChanges();
        Assert.IsTrue(project.Tasks.Contains(task));
    }

    [TestMethod]
    public void UpdateTask_WhenUpdatingExistingTask_ShouldUpdateTaskDetails()
    {
        var project = new Project { Name = "Project 1" , Description = "Project 1 Description"};
        _projectRepository.AddProject(project);
        _context.SaveChanges();

        var task = new Task("Task 1", "Task 1 description", DateTime.Now, 5, new List<Task>(), new List<Task>(), new List<Resource>()) { Id = 1, State = State.TODO };
        _projectRepository.AddTask(project.Name, task);
        _context.SaveChanges();

        var updatedTask = new Task("Updated Task 1", "Updated Task 1 description", DateTime.Now.AddDays(1), 10, new List<Task>(), new List<Task>(), new List<Resource>()) { Id = 1, State = State.DOING };
        _projectRepository.UpdateTask(project.Name, task.Id, updatedTask);
        _context.SaveChanges();

        var taskInProject = project.Tasks.FirstOrDefault(t => t.Id == task.Id);
        Assert.IsNotNull(taskInProject);
        Assert.AreEqual("Updated Task 1", taskInProject.Title);
        Assert.AreEqual("Updated Task 1 description", taskInProject.Description);
        Assert.AreEqual(State.DOING, taskInProject.State);
        Assert.AreEqual(10, taskInProject.Duration);
    }

    [TestMethod]
    [ExpectedException(typeof(ProjectNotFoundException))]
    public void UpdateTask_WhenProjectNotFound_ShouldThrowProjectNotFoundException()
    {
        var project = new Project { Name = "Project 1" };
        _projectRepository.AddProject(project);
        _context.SaveChanges();

        var task = new Task("Task 1", "Task 1 description", DateTime.Now, 5, new List<Task>(), new List<Task>(), new List<Resource>()) { Id = 1 };
        _projectRepository.AddTask(project.Name, task);
        _context.SaveChanges();

        var updatedTask = new Task("Updated Task 1", "Updated Task 1 description", DateTime.Now.AddDays(1), 10, new List<Task>(), new List<Task>(), new List<Resource>()) { Id = 1 };

        _projectRepository.UpdateTask("NonExistingProject", task.Id, updatedTask);
        _context.SaveChanges();
    }

    [TestMethod]
    [ExpectedException(typeof(TaskNotFoundException))]
    public void UpdateTask_WhenTaskNotFound_ShouldThrowTaskNotFoundException()
    {
        var project = new Project { Name = "Project 1" };
        _projectRepository.AddProject(project);
        _context.SaveChanges();

        var task = new Task("Task 1", "Task 1 description", DateTime.Now, 5, new List<Task>(), new List<Task>(), new List<Resource>()) { Id = 1 };
        _projectRepository.AddTask(project.Name, task);
        _context.SaveChanges();

        var updatedTask = new Task("Updated Task 1", "Updated Task 1 description", DateTime.Now.AddDays(1), 10, new List<Task>(), new List<Task>(), new List<Resource>()) { Id = 1 };

        _projectRepository.UpdateTask(project.Name, 999, updatedTask);
        _context.SaveChanges();
    }

    [TestMethod]
    public void RemoveTask_WhenTaskExists_ShouldRemoveTaskFromProject()
    {
        var project = new Project { Name = "Project 1", Description = "Project 1 Description"};
        _projectRepository.AddProject(project);
        _context.SaveChanges();

        var task = new Task("Task 1", "Task 1 description", DateTime.Now, 5, new List<Task>(), new List<Task>(), new List<Resource>()) { Id = 1 };
        _projectRepository.AddTask(project.Name, task);
        _context.SaveChanges();

        _projectRepository.RemoveTask(project.Name, task.Id);
        _context.SaveChanges();

        var taskInProject = project.Tasks.FirstOrDefault(t => t.Id == task.Id);
        Assert.IsNull(taskInProject);
    }

    [TestMethod]
    [ExpectedException(typeof(TaskNotFoundException))]
    public void RemoveTask_WhenTaskNotFound_ShouldThrowTaskNotFoundException()
    {
        var project = new Project { Name = "Project 1" };
        _projectRepository.AddProject(project);
        _context.SaveChanges();

        var task = new Task("Task 1", "Task 1 description", DateTime.Now, 5, new List<Task>(), new List<Task>(), new List<Resource>()) { Id = 1 };
        _projectRepository.AddTask(project.Name, task);
        _context.SaveChanges();

        _projectRepository.RemoveTask(project.Name, 999);
        _context.SaveChanges();
    }

    [TestMethod]
    [ExpectedException(typeof(ProjectNotFoundException))]
    public void AddPreviousTask_WhenProjectNotFound_ShouldThrowProjectNotFoundException()
    {
        var task = new Task("Task 1", "Task 1 description", DateTime.Now, 5, new List<Task>(), new List<Task>(), new List<Resource>()) { Id = 1 };
        var previousTask = new Task("Previous Task", "Previous Task description", DateTime.Now, 3, new List<Task>(), new List<Task>(), new List<Resource>()) { Id = 2 };

        _projectRepository.AddPreviousTask("NonExistingProject", task.Id, previousTask);
        _context.SaveChanges();
    }

    [TestMethod]
    [ExpectedException(typeof(TaskNotFoundException))]
    public void AddPreviousTask_WhenTaskNotFound_ShouldThrowTaskNotFoundException()
    {
        var project = new Project { Name = "Project 1" };
        _projectRepository.AddProject(project);
        _context.SaveChanges();

        var task = new Task("Task 1", "Task 1 description", DateTime.Now, 5, new List<Task>(), new List<Task>(), new List<Resource>()) { Id = 1 };
        var previousTask = new Task("Previous Task", "Previous Task description", DateTime.Now, 3, new List<Task>(), new List<Task>(), new List<Resource>()) { Id = 2 };

        _projectRepository.AddTask(project.Name, task);
        _context.SaveChanges();

        _projectRepository.AddPreviousTask(project.Name, 999, previousTask);
        _context.SaveChanges();
    }

    [TestMethod]
    [ExpectedException(typeof(TaskNotFoundException))]
    public void AddPreviousTask_WhenTaskNotPartOfProject_ShouldThrowTaskNotFoundException()
    {
        var project = new Project { Name = "Project 1" };
        _projectRepository.AddProject(project);
        _context.SaveChanges();

        var task = new Task("Task 1", "Task 1 description", DateTime.Now, 5, new List<Task>(), new List<Task>(), new List<Resource>()) { Id = 1 };
        var previousTask = new Task("Previous Task", "Previous Task description", DateTime.Now, 3, new List<Task>(), new List<Task>(), new List<Resource>()) { Id = 2 };

        _projectRepository.AddTask(project.Name, task);
        _context.SaveChanges();

        _projectRepository.AddPreviousTask(project.Name, task.Id, previousTask);
        _context.SaveChanges();
    }

    [TestMethod]
    public void AddPreviousTask_WhenTaskIsValid_ShouldAddPreviousTask()
    {
        var project = new Project { Name = "Project 1" };
        _projectRepository.AddProject(project);
        _context.SaveChanges();

        var task1 = new Task("Task 1", "Task 1 description", DateTime.Now, 5, new List<Task>(), new List<Task>(), new List<Resource>()) { Id = 1 };
        var task2 = new Task("Task 2", "Task 2 description", DateTime.Now, 3, new List<Task>(), new List<Task>(), new List<Resource>()) { Id = 2 };

        _projectRepository.AddTask(project.Name, task1);
        _projectRepository.AddTask(project.Name, task2);
        _context.SaveChanges();

        _projectRepository.AddPreviousTask(project.Name, task1.Id, task2);
        _context.SaveChanges();

        var taskInProject = project.Tasks.FirstOrDefault(t => t.Id == task1.Id);
        Assert.IsNotNull(taskInProject);
        Assert.AreEqual(1, taskInProject.PreviousTasks.Count);
        Assert.AreEqual(task2.Id, taskInProject.PreviousTasks[0].Id);
    }

    [TestMethod]
    public void AddResourceToTask_WhenTaskExists_ShouldAddResource()
    {
        var project = new Project { Name = "Project 1" };
        _projectRepository.AddProject(project);
        _context.SaveChanges();

        var task = new Task("Task 1", "Task 1 description", DateTime.Now, 5, new List<Task>(), new List<Task>(), new List<Resource>()) { Id = 1 };
        _projectRepository.AddTask(project.Name, task);
        _context.SaveChanges();

        var resource = new Resource("Resource 1", "Type 1", "Description of Resource 1");

        _projectRepository.AddResourceToTask(project.Name, task.Id, resource);
        _context.SaveChanges();

        var taskInProject = project.Tasks.FirstOrDefault(t => t.Id == task.Id);
        Assert.IsNotNull(taskInProject);
        Assert.IsTrue(taskInProject.Resources.Contains(resource));
    }

    [TestMethod]
    [ExpectedException(typeof(ProjectNotFoundException))]
    public void AddResourceToTask_WhenProjectNotFound_ShouldThrowProjectNotFoundException()
    {
        var task = new Task("Task 1", "Task 1 description", DateTime.Now, 5, new List<Task>(), new List<Task>(), new List<Resource>()) { Id = 1 };
        var resource = new Resource("Resource 1", "Type 1", "Description of Resource 1");

        _projectRepository.AddResourceToTask("NonExistingProject", task.Id, resource);
        _context.SaveChanges();
    }

    [TestMethod]
    [ExpectedException(typeof(TaskNotFoundException))]
    public void AddResourceToTask_WhenTaskNotFound_ShouldThrowTaskNotFoundException()
    {
        var project = new Project { Name = "Project 1" };
        _projectRepository.AddProject(project);
        _context.SaveChanges();

        var resource = new Resource("Resource 1", "Type 1", "Description of Resource 1");

        _projectRepository.AddResourceToTask(project.Name, 999, resource);
        _context.SaveChanges();
    }
}
