using DataAccess.ProjectRepositoryExceptions;
using Domain;
using Domain.Exceptions.TaskRepositoryExceptions;
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
    
    [TestMethod]
    [ExpectedException(typeof(TaskAlreadyExistsException))]
    public void AddTask_WhenTaskWithDuplicateTitle_ShouldThrowTaskAlreadyExistsException()
    {
        
        ProjectRepository projectRepository = new ProjectRepository();
        Project project = new Project() { Name = "Project 1" };
        projectRepository.AddProject(project);

        Task task1 = new Task("Task 1", "Description 1", DateTime.Now, 5, new List<Task>(), new List<Task>());
        projectRepository.AddTask(project.Name, task1);

       
        Task task2 = new Task("Task 1", "Description 2", DateTime.Now.AddDays(1), 3, new List<Task>(), new List<Task>());

      
        projectRepository.AddTask(project.Name, task2); 
    }
    
    [TestMethod]
    public void UpdateTask_WhenUpdatingExistingTask_ShouldUpdateTaskDetails()
    {
        ProjectRepository projectRepository = new ProjectRepository();
        Project project = new Project() { Name = "Project 1" };
        projectRepository.AddProject(project);

        Task task = new Task(
            "Task 1",                        
            "Task 1 description",            
            DateTime.Now,                    
            5,                               
            new List<Task>(),                
            new List<Task>()                 
        );
        task.Id = 1; 
        task.State = State.TODO; 

        projectRepository.AddTask(project.Name, task);

        Task updatedTask = new Task(
            "Updated Task 1",                
            "Updated Task 1 description",    
            DateTime.Now.AddDays(1),         
            10,                              
            new List<Task>(),                
            new List<Task>()                 
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
            ProjectRepository projectRepository = new ProjectRepository();
            Project project = new Project() { Name = "Project 1" };
            projectRepository.AddProject(project);

            Task task = new Task(
                "Task 1",                        
                "Task 1 description",            
                DateTime.Now,                    
                5,                               
                new List<Task>(),                
                new List<Task>()                 
            );
            task.Id = 1; 
            projectRepository.AddTask(project.Name, task);

            Task updatedTask = new Task(
                "Updated Task 1",                
                "Updated Task 1 description",    
                DateTime.Now.AddDays(1),         
                10,                              
                new List<Task>(),                
                new List<Task>()                 
            );
            updatedTask.Id = 1; 

            projectRepository.UpdateTask("NonExistingProject", task.Id, updatedTask);
        }

    
}