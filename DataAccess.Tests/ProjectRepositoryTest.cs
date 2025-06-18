using DataAccess.Exceptions.ProjectRepositoryExceptions;
using DataAccess.Exceptions.TaskRepositoryExceptions;
using Domain;
using Task = Domain.Task;

namespace DataAccess.Test;

[TestClass]
public class ProjectRepositoryTest
{
    private AppDbContext _context;
    private InMemoryAppContextFactory _contextFactory;
    private ProjectRepository _projectRepository;

    [TestInitialize]
    public void Setup()
    {
        _contextFactory = new InMemoryAppContextFactory();
        _context = _contextFactory.CreateDbContext();

        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();

        _projectRepository = new ProjectRepository(_context);
    }

    [TestCleanup]
    public void CleanUp()
    {
        _context?.Database.EnsureDeleted();
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
        _projectRepository.Add(project);
        var projects = _projectRepository.GetAll();
        Assert.IsTrue(projects.Any(p => p.Name == "Project 1"));
    }

    [TestMethod]
    [ExpectedException(typeof(DuplicatedProjectsNameException))]
    public void AddNewProject_WhenAddingDuplicatedProject_ShouldThrowDuplicatedNameException()
    {
        var project = new Project { Name = "Project 1", Description = "Project 1 description" };
        _projectRepository.Add(project);

        var duplicateProject = new Project { Name = "Project 1", Description = "Project 1 description" };
        _projectRepository.Add(duplicateProject);
    }

    [TestMethod]
    public void AddNewProject_WhenGettingProject_ShouldReturnProject()
    {
        var project = new Project { Name = "Project 1", Description = "Project 1 description" };
        var project2 = new Project { Name = "Project 2", Description = "Project 2 description" };

        _projectRepository.Add(project);
        _projectRepository.Add(project2);

        var foundProject = _projectRepository.Get(u => u.Name == "Project 2");
        Assert.IsNotNull(foundProject);
        Assert.AreEqual("Project 2", foundProject.Name);
        Assert.AreEqual("Project 2 description", foundProject.Description);
    }

    [TestMethod]
    public void DeleteProject_WhenGettingProject_ShouldBeNull()
    {
        var project = new Project { Name = "Project 1", Description = "Project 1 description" };
        var project2 = new Project { Name = "Project 2", Description = "Project 2 description" };

        _projectRepository.Add(project);
        _projectRepository.Add(project2);
        var p = _projectRepository.Get(p => p.Name == "Project 1");
        _projectRepository.Delete(p);

        Assert.IsNull(_projectRepository.Get(p => p.Name == "Project 1"));
    }

    [TestMethod]
    [ExpectedException(typeof(ProjectNotFoundException))]
    public void DeleteProject_WhenDeletingAgain_ShouldThrowProjectNotFoundException()
    {
        var project = new Project { Name = "Project 1", Description = "Project 1 description" };
        var project2 = new Project { Name = "Project 2", Description = "Project 2 description" };

        _projectRepository.Add(project);
        _projectRepository.Add(project2);
        var p = _projectRepository.Get(p => p.Name == "Project 1");

        _projectRepository.Delete(p);
        _projectRepository.Delete(p);
    }

    [TestMethod]
    public void UpdateProject_WhenGettingProject_ShouldBeDifferentFromTheOriginalProject()
    {
        var project = new Project { Name = "Project 1", Description = "Project 1 description" };
        _projectRepository.Add(project);

        var updatedProject = new Project
        {
            Name = "Project 1", Description = "Updated Project description", StartDate = DateTime.Parse("2026-01-01")
        };
        updatedProject.Id = _projectRepository.Get(p => p.Name == "Project 1").Id;
        _projectRepository.Update(updatedProject);

        var retrievedProject = _projectRepository.Get(p => p.Name == "Project 1");

        Assert.IsNotNull(retrievedProject);
        Assert.AreEqual("Updated Project description", retrievedProject.Description);
        Assert.AreEqual("Project 1", retrievedProject.Name);
    }

    [TestMethod]
    [ExpectedException(typeof(DuplicatedProjectsNameException))]
    public void updateProject_WhenNameIsDuplicated_ShouldThrowDuplicatedNameException()
    {
        var project = new Project { Name = "Project 1", Description = "Project 1 description" };
        var project2 = new Project { Name = "Project 2", Description = "Project 2 description" };
        _projectRepository.Add(project);
        _projectRepository.Add(project2);

        var project3 = new Project { Name = "Project 2", Description = "Project 3 description" };
        _projectRepository.Update(project3);
    }

    [TestMethod]
    [ExpectedException(typeof(ProjectNotFoundException))]
    public void updateProject_WhenNameIsNotFound_ShouldThrowProjectNotFoundException()
    {
        var project = new Project { Name = "Project 1", Description = "Project 1 description" };
        var project2 = new Project { Name = "Project 2", Description = "Project 2 description" };
        _projectRepository.Add(project);

        _projectRepository.Update(project2);
    }

    [TestMethod]
    public void AddTask_WhenAddingNewTask_ShouldContainIt()
    {
        var project = new Project { Name = "Project 1", Description = "Project 1 Description" };
        _projectRepository.Add(project);

        var task = new Task("Task 1", "Task 1 description", DateTime.Now, 5, new List<Task>(), new List<Task>(),
            new List<Resource>());
        _projectRepository.AddTask(project.Name, task);

        var refreshedProject = _projectRepository.Get(p => p.Name == "Project 1");
        Assert.IsNotNull(refreshedProject);
        Assert.IsTrue(refreshedProject.Tasks.Any(t => t.Title == "Task 1"));
    }

    [TestMethod]
    public void UpdateTask_WhenUpdatingExistingTask_ShouldUpdateTaskDetails()
    {
        var project = new Project { Name = "Project 1", Description = "Project 1 Description" };
        _projectRepository.Add(project);

        var task = new Task("Task 1", "Task 1 description", DateTime.Now, 5, new List<Task>(), new List<Task>(),
            new List<Resource>())
        {
            State = State.TODO
        };
        _projectRepository.AddTask(project.Name, task);

        var refreshedProject = _projectRepository.Get(p => p.Name == "Project 1");
        var insertedTask = refreshedProject.Tasks.First(t => t.Title == "Task 1");

        var updatedTask = new Task("Updated Task 1", "Updated Task 1 description", DateTime.Now.AddDays(1), 10,
            new List<Task>(), new List<Task>(), new List<Resource>())
        {
            State = State.DOING
        };

        _projectRepository.UpdateTask(project.Name, insertedTask.Id, updatedTask);

        var finalProject = _projectRepository.Get(p => p.Name == "Project 1");
        var taskInProject = finalProject.Tasks.FirstOrDefault(t => t.Id == insertedTask.Id);

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
        var project = new Project { Name = "Project 1", Description = "Project 1 Description" };
        _projectRepository.Add(project);

        var task = new Task("Task 1", "Task 1 description", DateTime.Now, 5, new List<Task>(), new List<Task>(),
            new List<Resource>());
        _projectRepository.AddTask(project.Name, task);

        var updatedTask = new Task("Updated Task 1", "Updated Task 1 description", DateTime.Now.AddDays(1), 10,
            new List<Task>(), new List<Task>(), new List<Resource>());

        _projectRepository.UpdateTask("NonExistingProject", 1, updatedTask);
    }

    [TestMethod]
    [ExpectedException(typeof(TaskNotFoundException))]
    public void UpdateTask_WhenTaskNotFound_ShouldThrowTaskNotFoundException()
    {
        var project = new Project { Name = "Project 1", Description = "Project 1 Description" };
        _projectRepository.Add(project);

        var task = new Task("Task 1", "Task 1 description", DateTime.Now, 5, new List<Task>(), new List<Task>(),
            new List<Resource>());
        _projectRepository.AddTask(project.Name, task);

        var updatedTask = new Task("Updated Task 1", "Updated Task 1 description", DateTime.Now.AddDays(1), 10,
            new List<Task>(), new List<Task>(), new List<Resource>());

        _projectRepository.UpdateTask(project.Name, 999, updatedTask);
    }

    [TestMethod]
    public void RemoveTask_WhenTaskExists_ShouldRemoveTaskFromProject()
    {
        var project = new Project { Name = "Project 1", Description = "Project 1 Description" };
        _projectRepository.Add(project);

        var task = new Task("Task 1", "Task 1 description", DateTime.Now, 5, new List<Task>(), new List<Task>(),
            new List<Resource>());
        _projectRepository.AddTask(project.Name, task);

        var refreshedProject = _projectRepository.Get(p => p.Name == "Project 1");
        var insertedTask = refreshedProject.Tasks.First(t => t.Title == "Task 1");

        _projectRepository.RemoveTask(project.Name, insertedTask.Id);

        var finalProject = _projectRepository.Get(p => p.Name == "Project 1");
        var taskInProject = finalProject.Tasks.FirstOrDefault(t => t.Id == insertedTask.Id);
        Assert.IsNull(taskInProject);
    }

    [TestMethod]
    [ExpectedException(typeof(TaskNotFoundException))]
    public void RemoveTask_WhenTaskNotFound_ShouldThrowTaskNotFoundException()
    {
        var project = new Project { Name = "Project 1", Description = "Project 1 Description" };
        _projectRepository.Add(project);

        var task = new Task("Task 1", "Task 1 description", DateTime.Now, 5, new List<Task>(), new List<Task>(),
            new List<Resource>());
        _projectRepository.AddTask(project.Name, task);

        _projectRepository.RemoveTask(project.Name, 999);
    }

    [TestMethod]
    [ExpectedException(typeof(ProjectNotFoundException))]
    public void AddPreviousTask_WhenProjectNotFound_ShouldThrowProjectNotFoundException()
    {
        var previousTask = new Task("Previous Task", "Previous Task description", DateTime.Now, 3, new List<Task>(),
            new List<Task>(), new List<Resource>());

        _projectRepository.AddPreviousTask("NonExistingProject", 1, previousTask);
    }

    [TestMethod]
    [ExpectedException(typeof(TaskNotFoundException))]
    public void AddPreviousTask_WhenTaskNotFound_ShouldThrowTaskNotFoundException()
    {
        var project = new Project { Name = "Project 1", Description = "Project 1 Description" };
        _projectRepository.Add(project);

        var task = new Task("Task 1", "Task 1 description", DateTime.Now, 5, new List<Task>(), new List<Task>(),
            new List<Resource>());
        _projectRepository.AddTask(project.Name, task);

        var previousTask = new Task("Previous Task", "Previous Task description", DateTime.Now, 3, new List<Task>(),
            new List<Task>(), new List<Resource>());

        _projectRepository.AddPreviousTask(project.Name, 999, previousTask);
    }

    [TestMethod]
    [ExpectedException(typeof(TaskNotFoundException))]
    public void AddPreviousTask_WhenTaskNotPartOfProject_ShouldThrowTaskNotFoundException()
    {
        var project = new Project { Name = "Project 1", Description = "Project 1 Description" };
        _projectRepository.Add(project);

        var task = new Task("Task 1", "Task 1 description", DateTime.Now, 5, new List<Task>(), new List<Task>(),
            new List<Resource>());
        _projectRepository.AddTask(project.Name, task);

        var refreshedProject = _projectRepository.Get(p => p.Name == "Project 1");
        var insertedTask = refreshedProject.Tasks.First(t => t.Title == "Task 1");

        var previousTask = new Task("Previous Task", "Previous Task description", DateTime.Now, 3, new List<Task>(),
            new List<Task>(), new List<Resource>());

        _projectRepository.AddPreviousTask(project.Name, insertedTask.Id, previousTask);
    }

    [TestMethod]
    public void AddPreviousTask_WhenTaskIsValid_ShouldAddPreviousTask()
    {
        var project = new Project { Name = "Project 1", Description = "Project 1 Description" };
        _projectRepository.Add(project);

        var task1 = new Task("Task 1", "Task 1 description", DateTime.Now, 5, new List<Task>(), new List<Task>(),
            new List<Resource>());
        var task2 = new Task("Task 2", "Task 2 description", DateTime.Now, 3, new List<Task>(), new List<Task>(),
            new List<Resource>());

        _projectRepository.AddTask(project.Name, task1);
        _projectRepository.AddTask(project.Name, task2);

        var refreshedProject = _projectRepository.Get(p => p.Name == "Project 1");
        var insertedTask1 = refreshedProject.Tasks.First(t => t.Title == "Task 1");
        var insertedTask2 = refreshedProject.Tasks.First(t => t.Title == "Task 2");

        _projectRepository.AddPreviousTask(project.Name, insertedTask1.Id, insertedTask2);

        var finalProject = _projectRepository.Get(p => p.Name == "Project 1");
        var taskInProject = finalProject.Tasks.FirstOrDefault(t => t.Id == insertedTask1.Id);

        Assert.IsNotNull(taskInProject);
        Assert.AreEqual(1, taskInProject.PreviousTasks.Count);
        Assert.AreEqual(insertedTask2.Id, taskInProject.PreviousTasks[0].Id);
    }

    [TestMethod]
    public void AddResourceToTask_WhenTaskExists_ShouldAddResource()
    {
        var project = new Project { Name = "Project 1", Description = "Project 1 Description" };
        _projectRepository.Add(project);

        var task = new Task("Task 1", "Task 1 description", DateTime.Now, 5, new List<Task>(), new List<Task>(),
            new List<Resource>());
        _projectRepository.AddTask(project.Name, task);

        var refreshedProject = _projectRepository.Get(p => p.Name == "Project 1");
        var insertedTask = refreshedProject.Tasks.First(t => t.Title == "Task 1");

        var resource = new Resource("Resource 1", "Type 1", "Description of Resource 1");
        _projectRepository.AddResourceToTask(project.Name, insertedTask.Id, resource);

        var finalProject = _projectRepository.Get(p => p.Name == "Project 1");
        var taskInProject = finalProject.Tasks.FirstOrDefault(t => t.Id == insertedTask.Id);

        Assert.IsNotNull(taskInProject);
        Assert.IsTrue(taskInProject.Resources.Any(r => r.Name == "Resource 1"));
    }

    [TestMethod]
    [ExpectedException(typeof(ProjectNotFoundException))]
    public void AddResourceToTask_WhenProjectNotFound_ShouldThrowProjectNotFoundException()
    {
        var resource = new Resource("Resource 1", "Type 1", "Description of Resource 1");

        _projectRepository.AddResourceToTask("NonExistingProject", 1, resource);
    }

    [TestMethod]
    [ExpectedException(typeof(TaskNotFoundException))]
    public void AddResourceToTask_WhenTaskNotFound_ShouldThrowTaskNotFoundException()
    {
        var project = new Project { Name = "Project 1", Description = "Project 1 Description" };
        _projectRepository.Add(project);

        var resource = new Resource("Resource 1", "Type 1", "Description of Resource 1");

        _projectRepository.AddResourceToTask(project.Name, 999, resource);
    }
}