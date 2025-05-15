using DataAccess.Exceptions.ProjectRepositoryExceptions;
using DataAccess.Exceptions.TaskRepositoryExceptions;
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
        var project = new Project();
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
        var project = new Project();
        project.Name = "Project 1";
        projectRepository.AddProject(project);
        projectRepository.AddProject(project);
    }

    [TestMethod]
    public void AddNewProject_WhenGettingProject_ShouldReturnProject()
    {
        ProjectRepository projectRepository;
        projectRepository = new ProjectRepository();
        var project = new Project();
        project.Name = "Project 1";
        var project2 = new Project();
        project2.Name = "Project 2";
        projectRepository.AddProject(project);
        projectRepository.AddProject(project2);
        Assert.AreEqual(projectRepository.GetProject(u => u.Name == "Project 2"), project2);
    }

    [TestMethod]
    public void DeleteProject_WhenGettingProject_ShouldBeNull()
    {
        ProjectRepository projectRepository;
        projectRepository = new ProjectRepository();
        var project = new Project();
        project.Name = "Project 1";
        var project2 = new Project();
        project2.Name = "Project 2";
        projectRepository.AddProject(project);
        projectRepository.AddProject(project2);
        projectRepository.RemoveProject(project.Name);
        Assert.IsNull(projectRepository.GetProject(p => p.Name == "Project 1"));
    }

    [TestMethod]
    [ExpectedException(typeof(ProjectNotFoundException))]
    public void DeleteProject_WhenDeletingAgain_ShouldThrowProjectNotFoundException()
    {
        ProjectRepository projectRepository;
        projectRepository = new ProjectRepository();
        var project = new Project();
        project.Name = "Project 1";
        var project2 = new Project();
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
        var project = new Project();
        project.Name = "Project 1";
        project.Description = "Project 1 description";
        var project2 = new Project();
        project2.Name = "Project 1";
        project2.Description = "Project 2 description";
        projectRepository.AddProject(project);
        projectRepository.UpdateProject(project.Name, project2);
        Assert.AreNotEqual(projectRepository.GetProject(p => p.Name == "Project 1"), project);
    }

    [TestMethod]
    [ExpectedException(typeof(DuplicatedProjectsNameException))]
    public void updateProject_WhenNameIsDuplicated_ShouldThrowDuplicatedNameException()
    {
        ProjectRepository projectRepository;
        projectRepository = new ProjectRepository();
        var project = new Project();
        project.Name = "Project 1";
        project.Description = "Project 1 description";
        var project2 = new Project();
        project2.Name = "Project 2";
        project2.Description = "Project 2 description";
        var project3 = new Project();
        project3.Name = "Project 2";
        project3.Description = "Project 3 description";
        projectRepository.AddProject(project);
        projectRepository.AddProject(project2);
        projectRepository.UpdateProject(project.Name, project3);
    }

    [TestMethod]
    [ExpectedException(typeof(ProjectNotFoundException))]
    public void updateProject_WhenNameIsNotFound_ShouldThrowProjectNotFoundException()
    {
        ProjectRepository projectRepository;
        projectRepository = new ProjectRepository();
        var project = new Project();
        project.Name = "Project 1";
        project.Description = "Project 1 description";
        var project2 = new Project();
        project2.Name = "Project 2";
        project2.Description = "Project 2 description";
        var project3 = new Project();
        projectRepository.AddProject(project);
        projectRepository.UpdateProject("Project 4", project2);
    }

    [TestMethod]
    public void AddTask_WhenAddingNewTask_ShouldContainIt()
    {
        var projectRepository = new ProjectRepository();
        var project = new Project();
        project.Name = "Project 1";

        var task = new Task(
            "Task 1",
            "Task 1 description",
            DateTime.Now,
            5,
            new List<Task>(),
            new List<Task>(),
            new List<Resource>()
        );

        projectRepository.AddProject(project);
        projectRepository.AddTask(project.Name, task);

        Assert.IsTrue(project.Tasks.Contains(task));
    }

    [TestMethod]
    public void UpdateTask_WhenUpdatingExistingTask_ShouldUpdateTaskDetails()
    {
        var projectRepository = new ProjectRepository();
        var project = new Project { Name = "Project 1" };
        projectRepository.AddProject(project);

        var task = new Task(
            "Task 1",
            "Task 1 description",
            DateTime.Now,
            5,
            new List<Task>(),
            new List<Task>(),
            new List<Resource>()
        );
        task.Id = 1;
        task.State = State.TODO;

        projectRepository.AddTask(project.Name, task);

        var updatedTask = new Task(
            "Updated Task 1",
            "Updated Task 1 description",
            DateTime.Now.AddDays(1),
            10,
            new List<Task>(),
            new List<Task>(),
            new List<Resource>()
        );
        updatedTask.Id = 1;
        updatedTask.State = State.DOING;

        projectRepository.UpdateTask(project.Name, task.Id, updatedTask);

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
        var projectRepository = new ProjectRepository();
        var project = new Project { Name = "Project 1" };
        projectRepository.AddProject(project);

        var task = new Task(
            "Task 1",
            "Task 1 description",
            DateTime.Now,
            5,
            new List<Task>(),
            new List<Task>(),
            new List<Resource>()
        );
        task.Id = 1;
        projectRepository.AddTask(project.Name, task);

        var updatedTask = new Task(
            "Updated Task 1",
            "Updated Task 1 description",
            DateTime.Now.AddDays(1),
            10,
            new List<Task>(),
            new List<Task>(),
            new List<Resource>()
        );
        updatedTask.Id = 1;

        projectRepository.UpdateTask("NonExistingProject", task.Id, updatedTask);
    }

    [TestMethod]
    [ExpectedException(typeof(TaskNotFoundException))]
    public void UpdateTask_WhenTaskNotFound_ShouldThrowTaskNotFoundException()
    {
        var projectRepository = new ProjectRepository();
        var project = new Project { Name = "Project 1" };
        projectRepository.AddProject(project);

        var task = new Task(
            "Task 1",
            "Task 1 description",
            DateTime.Now,
            5,
            new List<Task>(),
            new List<Task>(),
            new List<Resource>()
        );
        task.Id = 1;
        projectRepository.AddTask(project.Name, task);

        var updatedTask = new Task(
            "Updated Task 1",
            "Updated Task 1 description",
            DateTime.Now.AddDays(1),
            10,
            new List<Task>(),
            new List<Task>(),
            new List<Resource>()
        );
        updatedTask.Id = 1;

        projectRepository.UpdateTask(project.Name, 999, updatedTask);
    }

    [TestMethod]
    public void RemoveTask_WhenTaskExists_ShouldRemoveTaskFromProject()
    {
        var projectRepository = new ProjectRepository();
        var project = new Project { Name = "Project 1" };
        projectRepository.AddProject(project);

        var task = new Task(
            "Task 1",
            "Task 1 description",
            DateTime.Now,
            5,
            new List<Task>(),
            new List<Task>(),
            new List<Resource>()
        );
        task.Id = 1;
        projectRepository.AddTask(project.Name, task);

        projectRepository.RemoveTask(project.Name, task.Id);

        var taskInProject = project.Tasks.FirstOrDefault(t => t.Id == task.Id);
        Assert.IsNull(taskInProject);
    }

    [TestMethod]
    [ExpectedException(typeof(TaskNotFoundException))]
    public void RemoveTask_WhenTaskNotFound_ShouldThrowTaskNotFoundException()
    {
        var projectRepository = new ProjectRepository();
        var project = new Project { Name = "Project 1" };
        projectRepository.AddProject(project);

        var task = new Task(
            "Task 1",
            "Task 1 description",
            DateTime.Now,
            5,
            new List<Task>(),
            new List<Task>(),
            new List<Resource>()
        );
        task.Id = 1;
        projectRepository.AddTask(project.Name, task);

        projectRepository.RemoveTask(project.Name, 999);
    }

    [TestMethod]
    [ExpectedException(typeof(ProjectNotFoundException))]
    public void AddPreviousTask_WhenProjectNotFound_ShouldThrowProjectNotFoundException()
    {
        var projectRepository = new ProjectRepository();
        var task = new Task(
            "Task 1",
            "Task 1 description",
            DateTime.Now,
            5,
            new List<Task>(),
            new List<Task>(),
            new List<Resource>()
        );
        task.Id = 1;

        var previousTask = new Task(
            "Previous Task",
            "Previous Task description",
            DateTime.Now,
            3,
            new List<Task>(),
            new List<Task>(),
            new List<Resource>()
        );
        previousTask.Id = 2;

        projectRepository.AddPreviousTask("NonExistingProject", task.Id, previousTask);
    }

    [TestMethod]
    [ExpectedException(typeof(TaskNotFoundException))]
    public void AddPreviousTask_WhenTaskNotFound_ShouldThrowTaskNotFoundException()
    {
        var projectRepository = new ProjectRepository();
        var project = new Project { Name = "Project 1" };
        projectRepository.AddProject(project);

        var task = new Task(
            "Task 1",
            "Task 1 description",
            DateTime.Now,
            5,
            new List<Task>(),
            new List<Task>(),
            new List<Resource>()
        );
        task.Id = 1;

        var previousTask = new Task(
            "Previous Task",
            "Previous Task description",
            DateTime.Now,
            3,
            new List<Task>(),
            new List<Task>(),
            new List<Resource>()
        );
        previousTask.Id = 2;

        projectRepository.AddTask(project.Name, task);

        projectRepository.AddPreviousTask(project.Name, 999, previousTask);
    }

    [TestMethod]
    [ExpectedException(typeof(TaskNotFoundException))]
    public void AddPreviousTask_WhenTaskNotPartOfProject_ShouldThrowTaskNotFoundException()
    {
        var projectRepository = new ProjectRepository();
        var project = new Project { Name = "Project 1" };
        projectRepository.AddProject(project);

        var task = new Task(
            "Task 1",
            "Task 1 description",
            DateTime.Now,
            5,
            new List<Task>(),
            new List<Task>(),
            new List<Resource>()
        );
        task.Id = 1;

        var previousTask = new Task(
            "Previous Task",
            "Previous Task description",
            DateTime.Now,
            3,
            new List<Task>(),
            new List<Task>(),
            new List<Resource>()
        );
        previousTask.Id = 2;

        projectRepository.AddTask(project.Name, task);

        projectRepository.AddPreviousTask(project.Name, task.Id, previousTask);
    }

    [TestMethod]
    public void AddPreviousTask_WhenTaskIsValid_ShouldAddPreviousTask()
    {
        var projectRepository = new ProjectRepository();
        var project = new Project { Name = "Project 1" };
        projectRepository.AddProject(project);

        var task1 = new Task(
            "Task 1",
            "Task 1 description",
            DateTime.Now,
            5,
            new List<Task>(),
            new List<Task>(),
            new List<Resource>()
        );
        task1.Id = 1;

        var task2 = new Task(
            "Task 2",
            "Task 2 description",
            DateTime.Now,
            3,
            new List<Task>(),
            new List<Task>(),
            new List<Resource>()
        );
        task2.Id = 2;

        projectRepository.AddTask(project.Name, task1);
        projectRepository.AddTask(project.Name, task2);

        projectRepository.AddPreviousTask(project.Name, task1.Id, task2);

        var taskInProject = project.Tasks.FirstOrDefault(t => t.Id == task1.Id);
        Assert.IsNotNull(taskInProject);
        Assert.AreEqual(1, taskInProject.PreviousTasks.Count);
        Assert.AreEqual(task2.Id, taskInProject.PreviousTasks[0].Id);
    }


    [TestMethod]
    public void AddResourceToTask_WhenTaskExists_ShouldAddResource()
    {
        var projectRepository = new ProjectRepository();
        var project = new Project { Name = "Project 1" };
        projectRepository.AddProject(project);

        var task = new Task(
            "Task 1",
            "Task 1 description",
            DateTime.Now,
            5,
            new List<Task>(),
            new List<Task>(),
            new List<Resource>()
        );
        task.Id = 1;
        projectRepository.AddTask(project.Name, task);

        var resource = new Resource("Resource 1", "Type 1", "Description of Resource 1");

        projectRepository.AddResourceToTask(project.Name, task.Id, resource);

        var taskInProject = project.Tasks.FirstOrDefault(t => t.Id == task.Id);
        Assert.IsNotNull(taskInProject);
        Assert.IsTrue(taskInProject.Resources.Contains(resource));
    }

    [TestMethod]
    [ExpectedException(typeof(ProjectNotFoundException))]
    public void AddResourceToTask_WhenProjectNotFound_ShouldThrowProjectNotFoundException()
    {
        var projectRepository = new ProjectRepository();
        var task = new Task(
            "Task 1",
            "Task 1 description",
            DateTime.Now,
            5,
            new List<Task>(),
            new List<Task>(),
            new List<Resource>()
        );
        task.Id = 1;

        var resource = new Resource("Resource 1", "Type 1", "Description of Resource 1");

        projectRepository.AddResourceToTask("NonExistingProject", task.Id, resource);
    }


    [TestMethod]
    [ExpectedException(typeof(TaskNotFoundException))]
    public void AddResourceToTask_WhenTaskNotFound_ShouldThrowTaskNotFoundException()
    {
        var projectRepository = new ProjectRepository();
        var project = new Project { Name = "Project 1" };
        projectRepository.AddProject(project);

        var resource = new Resource("Resource 1", "Type 1", "Description of Resource 1");

        projectRepository.AddResourceToTask(project.Name, 999, resource); // Task with ID 999 does not exist
    }
}