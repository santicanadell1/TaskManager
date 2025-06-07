using DataAccess;
using DataAccess.Exceptions.ResourceRepositoryExceptions;
using Domain;
using Service.Exceptions.AdminSServiceExceptions;
using Service.Models;
using Task = Domain.Task;

namespace Service.Test;

[TestClass]
public class ResourcesServiceTest
{
    private AdminPService _adminProjectService;
    private AppDbContext _context;
    private Login _loginService;
    private ResourceService _resourceService;
    private TaskService _taskService;
    private UserService _userService;
    private InMemoryAppContextFactory _contextFactory;
    private IRepositoryManager _repositoryManager;

    [TestInitialize]
    public void TestSetUp()
    {
        _contextFactory = new InMemoryAppContextFactory();
        _context = _contextFactory.CreateDbContext();

        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();

        _repositoryManager = new RepositoryManager(_context);
        
        _loginService = new Login(_repositoryManager);
        _userService = new UserService(_repositoryManager);
        _resourceService = new ResourceService(_repositoryManager);
        _adminProjectService =
            new AdminPService(_repositoryManager);

        CpmService cpmService = new CpmService();

        _taskService = new TaskService(_repositoryManager,cpmService);

        UserDTO adminSUserDTO = new UserDTO
        {
            FirstName = "AdminSystem",
            LastName = "User",
            Email = "adminSystem.user@example.com",
            Password = "AdminPassword123@",
            Birthday = DateTime.Parse("1990-01-01"),
            Roles = new List<RolDTO> { RolDTO.AdminSystem }
        };
        UserDTO adminPUserDTO = new UserDTO
        {
            FirstName = "AdminProject",
            LastName = "User",
            Email = "adminProject.user@example.com",
            Password = "AdminPassword123@",
            Birthday = DateTime.Parse("1990-01-01"),
            Roles = new List<RolDTO> { RolDTO.AdminProject }
        };


        UserDTO adminPUserDTO2 = new UserDTO
        {
            FirstName = "AdminProject2",
            LastName = "User2",
            Email = "adminProject2.user@example.com",
            Password = "AdminPassword123@",
            Birthday = DateTime.Parse("1990-01-01"),
            Roles = new List<RolDTO> { RolDTO.AdminProject }
        };

        UserDTO normalUserDTO = new UserDTO
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Password = "Password123@",
            Birthday = DateTime.Parse("1990-01-01"),
            Roles = new List<RolDTO>()
        };
        _userService.AddUser(normalUserDTO);
        _userService.AddUser(adminSUserDTO);
        _userService.AddUser(adminPUserDTO);
        _userService.AddUser(adminPUserDTO2);
    }

    [TestMethod]
    public void AddResource_ShouldAddResource_WhenValidDTO()
    {
        _loginService.LoginUser("adminSystem.user@example.com", "AdminPassword123@");
        ResourceDTO resourceDTO = new ResourceDTO
        {
            Name = "Resource1",
            Type = "TypeA",
            Description = "Description of Resource1"
        };

        _resourceService.AddResource(resourceDTO);

        Resource resource = _repositoryManager.ResourceRepository.Get(r => r.Name == "Resource1");
        Assert.IsNotNull(resource);
        Assert.AreEqual("Resource1", resource.Name);
        Assert.AreEqual("TypeA", resource.Type);
        Assert.AreEqual("Description of Resource1", resource.Description);
    }

    [TestMethod]
    public void Get_ShouldReturnResource_WhenResourceExists()
    {
        ResourceDTO resourceDTO = new ResourceDTO
        {
            Name = "Resource1",
            Type = "TypeA",
            Description = "Description of Resource1"
        };

        _resourceService.AddResource(resourceDTO);

        Resource addedResource = _repositoryManager.ResourceRepository.Get(r => r.Name == "Resource1");

        ResourceDTO resource = _resourceService.Get(addedResource.Id);

        Assert.IsNotNull(resource);
        Assert.AreEqual("Resource1", resource.Name);
        Assert.AreEqual("TypeA", resource.Type);
        Assert.AreEqual("Description of Resource1", resource.Description);
    }

    [TestMethod]
    [ExpectedException(typeof(ResourceNotFoundException))]
    public void Get_ShouldThrowException_WhenResourceDoesNotExist()
    {
        _resourceService.Get(999);
    }

    [TestMethod]
    public void GetResources_ShouldReturnAllResources_WhenResourcesExist()
    {
        ResourceDTO resourceDTO1 = new ResourceDTO
        {
            Name = "Resource1",
            Type = "TypeA",
            Description = "Description of Resource1"
        };

        ResourceDTO resourceDTO2 = new ResourceDTO
        {
            Name = "Resource2",
            Type = "TypeB",
            Description = "Description of Resource2"
        };

        _resourceService.AddResource(resourceDTO1);
        _resourceService.AddResource(resourceDTO2);

        List<ResourceDTO> resources = _resourceService.GetResources();

        Assert.AreEqual(2, resources.Count);
        Assert.IsTrue(resources.Exists(r => r.Name == "Resource1"));
        Assert.IsTrue(resources.Exists(r => r.Name == "Resource2"));
    }

    [TestMethod]
    public void UpdateResource_ShouldUpdateResource_WhenResourceExists()
    {
        _loginService.LoginUser("adminSystem.user@example.com", "AdminPassword123@");
        ResourceDTO resourceDTO = new ResourceDTO
        {
            Name = "Resource1",
            Type = "TypeA",
            Description = "Description of Resource1"
        };

        _resourceService.AddResource(resourceDTO);

        Resource addedResource = _repositoryManager.ResourceRepository.Get(r => r.Name == "Resource1");

        ResourceDTO updatedResourceDTO = new ResourceDTO
        {
            Name = "Resource1",
            Type = "TypeB",
            Description = "Updated description"
        };

        _resourceService.UpdateResource(addedResource.Id, updatedResourceDTO);

        Resource resource = _repositoryManager.ResourceRepository.Get(r =>
            r.Name == updatedResourceDTO.Name && r.Type == updatedResourceDTO.Type &&
            r.Description == updatedResourceDTO.Description);

        Assert.IsNotNull(resource);
        Assert.AreEqual("TypeB", resource.Type);
        Assert.AreEqual("Updated description", resource.Description);
    }

    [TestMethod]
    public void UpdateResource_ShouldUpdateResource_WhenResourceIsExclusive()
    {
        _loginService.LoginUser("adminSystem.user@example.com", "AdminPassword123@");
        ResourceDTO resourceDTO = new ResourceDTO
        {
            Name = "Resource1",
            Type = "TypeA",
            Description = "Description of Resource1"
        };

        _resourceService.AddResource(resourceDTO);
        _loginService.LoginUser("adminProject.user@example.com", "AdminPassword123@");
        Resource addedResource = _repositoryManager.ResourceRepository.Get(r => r.Name == "Resource1");

        ResourceDTO addedResourceDto = new ResourceDTO
        {
            Id = addedResource.Id,
            Name = addedResource.Name,
            Type = addedResource.Type,
            Description = addedResource.Description
        };

        ProjectDTO project = new ProjectDTO();
        project.Name = "Project1";
        project.Description = "Description of Project1";
        project.StartDate = DateTime.Today;
        project.AdminProyect = _userService.GetUser("adminProject.user@example.com");

        TaskDTO task = new TaskDTO
        {
            Title = "Title1",
            Description = "Description1",
            ExpectedStartDate = DateTime.Today,
            Duration = 5,
            PreviousTasks = new List<TaskDTO>(),
            SameTimeTasks = new List<TaskDTO>(),
            Resources = new List<ResourceDTO> { addedResourceDto }
        };

        _adminProjectService.CreateProject(project);
        _taskService.AddTask("Project1", task);

        ResourceDTO updatedResourceDTO = new ResourceDTO
        {
            Name = "Resource1",
            Type = "TypeB",
            Description = "Updated description",
        };

        _resourceService.UpdateResource(addedResource.Id, updatedResourceDTO);

        Resource resource = _repositoryManager.ResourceRepository.Get(r =>
            r.Name == updatedResourceDTO.Name && r.Type == updatedResourceDTO.Type &&
            r.Description == updatedResourceDTO.Description);

        Assert.IsNotNull(resource);
        Assert.AreEqual("TypeB", resource.Type);
        Assert.AreEqual("Updated description", resource.Description);
    }


    [TestMethod]
    [ExpectedException(typeof(UnauthorizedAdminAccessException))]
    public void UpdateResource_ShouldThrowException_WhenResourceIsNotExclusive()
    {
        _loginService.Logout();
        _loginService.LoginUser("adminProject.user@example.com", "AdminPassword123@");

        ResourceDTO resourceDTO = new ResourceDTO
        {
            Name = "Resource1",
            Type = "TypeA",
            Description = "Description of Resource1"
        };

        _resourceService.AddResource(resourceDTO);

        _loginService.Logout();
        _loginService.LoginUser("adminProject.user@example.com", "AdminPassword123@");
        Resource addedResource = _repositoryManager.ResourceRepository.Get(r => r.Name == "Resource1");

        ResourceDTO addedResourceDto = _resourceService.Get(addedResource.Id);

        ProjectDTO project = new ProjectDTO();
        project.Name = "Project 1";
        project.Description = "Description of Project1";
        project.StartDate = DateTime.Today;

        ProjectDTO project2 = new ProjectDTO();
        project2.Name = "Project 2";
        project2.Description = "Description of Project2";
        project2.StartDate = DateTime.Today;

        TaskDTO task = new TaskDTO
        {
            Title = "Title 1",
            Description = "Description1",
            ExpectedStartDate = DateTime.Today,
            Duration = 5,
            PreviousTasks = new List<TaskDTO>(),
            SameTimeTasks = new List<TaskDTO>(),
            Resources = new List<ResourceDTO> { addedResourceDto }
        };

        TaskDTO task2 = new TaskDTO
        {
            Title = "Title 2",
            Description = "Description2",
            ExpectedStartDate = DateTime.Today,
            Duration = 5,
            PreviousTasks = new List<TaskDTO>(),
            SameTimeTasks = new List<TaskDTO>(),
            Resources = new List<ResourceDTO> { addedResourceDto }
        };

        _adminProjectService.CreateProject(project);
        _taskService.AddTask("Project 1", task);
        _adminProjectService.CreateProject(project2);
        _taskService.AddTask("Project 2", task2);

        ResourceDTO updatedResourceDTO = new ResourceDTO
        {
            Name = "Resource1",
            Type = "TypeB",
            Description = "Updated description"
        };


        _resourceService.UpdateResource(addedResourceDto.Id, updatedResourceDTO);
    }

    [TestMethod]
    [ExpectedException(typeof(UnauthorizedAdminAccessException))]
    public void UpdateResource_ShouldThrowException_WhenResourceIsNotExclusiveForTheAdmin()
    {
        _loginService.LoginUser("adminSystem.user@example.com", "AdminPassword123@");
        ResourceDTO resourceDTO = new ResourceDTO
        {
            Name = "Resource1",
            Type = "TypeA",
            Description = "Description of Resource1"
        };

        _resourceService.AddResource(resourceDTO);
        _loginService.LoginUser("adminProject.user@example.com", "AdminPassword123@");
        Resource addedResource = _repositoryManager.ResourceRepository.Get(r => r.Name == "Resource1");
        ResourceDTO addedResourceDto = new ResourceDTO
        {
            Id = addedResource.Id,
            Name = addedResource.Name,
            Type = addedResource.Type,
            Description = addedResource.Description
        };

        ProjectDTO project = new ProjectDTO();
        project.Name = "Project1";
        project.Description = "Description of Project1";
        project.StartDate = DateTime.Today;
        project.AdminProyect = _userService.GetUser("adminProject.user@example.com");

        TaskDTO task = new TaskDTO
        {
            Title = "Title1",
            Description = "Description1",
            ExpectedStartDate = DateTime.Today,
            Duration = 5,
            PreviousTasks = new List<TaskDTO>(),
            SameTimeTasks = new List<TaskDTO>(),
            Resources = new List<ResourceDTO> { addedResourceDto }
        };
        _adminProjectService.CreateProject(project);
        _taskService.AddTask("Project1", task);

        ResourceDTO updatedResourceDTO = new ResourceDTO
        {
            Name = "Resource1",
            Type = "TypeB",
            Description = "Updated description"
        };

        _loginService.LoginUser("adminProject2.user@example.com", "AdminPassword123@");
        _resourceService.UpdateResource(addedResource.Id, updatedResourceDTO);
    }

    [TestMethod]
    public void DeleteResource_ShouldDeleteResource_WhenResourceExists()
    {
        _loginService.LoginUser("adminSystem.user@example.com", "AdminPassword123@");
        ResourceDTO resourceDTO = new ResourceDTO
        {
            Name = "ResourceToDelete",
            Type = "TypeA",
            Description = "Description of resource to delete"
        };

        _resourceService.AddResource(resourceDTO);

        Resource addedResource = _repositoryManager.ResourceRepository.Get(r => r.Name == resourceDTO.Name);
        Assert.IsNotNull(addedResource);

        _resourceService.DeleteResource(addedResource.Id);

        Resource deletedResource = _repositoryManager.ResourceRepository.Get(r => r.Name == resourceDTO.Name);
        Assert.IsNull(deletedResource);
    }

    [TestMethod]
    public void DeleteResource_ShouldDeleteResource_WhenResourceIsExclusive()
    {
        _loginService.LoginUser("adminSystem.user@example.com", "AdminPassword123@");
        ResourceDTO resourceDTO = new ResourceDTO
        {
            Name = "Resource1",
            Type = "TypeA",
            Description = "Description of Resource1"
        };

        _resourceService.AddResource(resourceDTO);
        _context.ChangeTracker.Clear();

        _loginService.LoginUser("adminProject.user@example.com", "AdminPassword123@");
        Resource addedResource = _repositoryManager.ResourceRepository.Get(r => r.Name == "Resource1");

        ProjectDTO project = new ProjectDTO();
        project.Name = "Project1";
        project.Description = "Description of Project1";
        project.StartDate = DateTime.Today;
        project.AdminProyect = _userService.GetUser("adminProject.user@example.com");

        _adminProjectService.CreateProject(project);

        TaskDTO task = new TaskDTO
        {
            Title = "Title1",
            Description = "Description1",
            ExpectedStartDate = DateTime.Today,
            Duration = 5,
            PreviousTasks = new List<TaskDTO>(),
            SameTimeTasks = new List<TaskDTO>(),
            Resources = new List<ResourceDTO>()
        };
        _taskService.AddTask("Project1", task);

        Project projectEntity = _repositoryManager.ProjectRepository.Get(p => p.Name == "Project1");
        Task taskEntity = projectEntity.Tasks.First(t => t.Title == "Title1");
        Resource resourceEntity = _repositoryManager.ResourceRepository.Get(r => r.Id == addedResource.Id);

        taskEntity.Resources.Add(resourceEntity);
        _context.SaveChanges();

        _resourceService.DeleteResource(addedResource.Id);

        Resource deletedResource = _repositoryManager.ResourceRepository.Get(r => r.Name == resourceDTO.Name);
        Assert.IsNull(deletedResource);
    }

    [TestMethod]
    [ExpectedException(typeof(UnauthorizedAdminAccessException))]
    public void DeleteResource_ShouldThrowException_WhenResourceIsNotExclusive()
    {
        _loginService.Logout();
        _loginService.LoginUser("adminProject.user@example.com", "AdminPassword123@");

        ResourceDTO resourceDTO = new ResourceDTO
        {
            Name = "Resource1",
            Type = "TypeA",
            Description = "Description of Resource1"
        };

        _resourceService.AddResource(resourceDTO);
        _loginService.Logout();
        _loginService.LoginUser("adminProject.user@example.com", "AdminPassword123@");
        Resource addedResource = _repositoryManager.ResourceRepository.Get(r => r.Name == "Resource1");

        ResourceDTO addedResourceDto = _resourceService.Get(addedResource.Id);

        ProjectDTO project = new ProjectDTO();
        project.Name = "Project 1";
        project.Description = "Description of Project1";
        project.StartDate = DateTime.Today;

        ProjectDTO project2 = new ProjectDTO();
        project2.Name = "Project 2";
        project2.Description = "Description of Project2";
        project2.StartDate = DateTime.Today;

        TaskDTO task = new TaskDTO
        {
            Title = "Title 1",
            Description = "Description1",
            ExpectedStartDate = DateTime.Today,
            Duration = 5,
            PreviousTasks = new List<TaskDTO>(),
            SameTimeTasks = new List<TaskDTO>(),
            Resources = new List<ResourceDTO> { addedResourceDto }
        };

        TaskDTO task2 = new TaskDTO
        {
            Title = "Title 2",
            Description = "Description2",
            ExpectedStartDate = DateTime.Today,
            Duration = 5,
            PreviousTasks = new List<TaskDTO>(),
            SameTimeTasks = new List<TaskDTO>(),
            Resources = new List<ResourceDTO> { addedResourceDto }
        };

        _adminProjectService.CreateProject(project);
        _taskService.AddTask("Project 1", task);
        _adminProjectService.CreateProject(project2);
        _taskService.AddTask("Project 2", task2);

        _resourceService.DeleteResource(addedResource.Id);
    }

    [TestMethod]
    [ExpectedException(typeof(UnauthorizedAdminAccessException))]
    public void DeleteResource_ShouldThrowExceptin_WhenResourceIsNotExclusiveForTheAdmin()
    {
        _loginService.LoginUser("adminSystem.user@example.com", "AdminPassword123@");
        ResourceDTO resourceDTO = new ResourceDTO
        {
            Name = "Resource1",
            Type = "TypeA",
            Description = "Description of Resource1"
        };

        _resourceService.AddResource(resourceDTO);

        _loginService.LoginUser("adminProject.user@example.com", "AdminPassword123@");
        Resource addedResource = _repositoryManager.ResourceRepository.Get(r => r.Name == "Resource1");

        ResourceDTO addedResourceDto = new ResourceDTO
        {
            Id = addedResource.Id,
            Name = addedResource.Name,
            Type = addedResource.Type,
            Description = addedResource.Description
        };


        ProjectDTO project = new ProjectDTO();
        project.Name = "Project1";
        project.Description = "Description of Project1";
        project.StartDate = DateTime.Today;
        project.AdminProyect = _userService.GetUser("adminProject.user@example.com");

        TaskDTO task = new TaskDTO
        {
            Title = "Title1",
            Description = "Description1",
            ExpectedStartDate = DateTime.Today,
            Duration = 5,
            PreviousTasks = new List<TaskDTO>(),
            SameTimeTasks = new List<TaskDTO>(),
            Resources = new List<ResourceDTO> { addedResourceDto }
        };
        _adminProjectService.CreateProject(project);

        _taskService.AddTask("Project1", task);

        Assert.IsNotNull(addedResource);
        _loginService.LoginUser("adminProject2.user@example.com", "AdminPassword123@");

        _resourceService.DeleteResource(addedResource.Id);
    }

    [TestMethod]
    [ExpectedException(typeof(ResourceNotFoundException))]
    public void DeleteResource_ShouldThroException_WhenResourceNoExists()
    {
        _resourceService.DeleteResource(999);
    }
}