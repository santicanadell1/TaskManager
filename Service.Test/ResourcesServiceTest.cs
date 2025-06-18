using System;
using System.Collections.Generic;
using System.Linq;
using DataAccess;
using DataAccess.Exceptions.ResourceRepositoryExceptions;
using Domain;
using Service;
using Service.Exceptions.AdminSServiceExceptions;
using Service.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Task = Domain.Task;

namespace Service.Test;

[TestClass]
public class ResourcesServiceTest
{
    private AdminPService _adminProjectService;
    private AppDbContext _context;
    private InMemoryAppContextFactory _contextFactory;
    private Login _loginService;
    private IRepositoryManager _repositoryManager;
    private ResourceService _resourceService;
    private TaskService _taskService;
    private UserService _userService;

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
        _adminProjectService = new AdminPService(_repositoryManager);

        var cpmService = new CpmService();
        _taskService = new TaskService(_repositoryManager, cpmService);

        var adminSUserDTO = new UserDTO
        {
            FirstName = "AdminSystem",
            LastName = "User",
            Email = "adminSystem.user@example.com",
            Password = "AdminPassword123@",
            Birthday = DateTime.Parse("1990-01-01"),
            Roles = new List<RolDTO> { RolDTO.AdminSystem }
        };
        var adminPUserDTO = new UserDTO
        {
            FirstName = "AdminProject",
            LastName = "User",
            Email = "adminProject.user@example.com",
            Password = "AdminPassword123@",
            Birthday = DateTime.Parse("1990-01-01"),
            Roles = new List<RolDTO> { RolDTO.AdminProject }
        };
        var adminPUserDTO2 = new UserDTO
        {
            FirstName = "AdminProject2",
            LastName = "User2",
            Email = "adminProject2.user@example.com",
            Password = "AdminPassword123@",
            Birthday = DateTime.Parse("1990-01-01"),
            Roles = new List<RolDTO> { RolDTO.AdminProject }
        };
        var normalUserDTO = new UserDTO
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
        var resourceDTO = new ResourceDTO
        {
            Name = "Resource1",
            Type = "TypeA",
            Description = "Description of Resource1"
        };

        _resourceService.AddResource(resourceDTO);

        var resource = _repositoryManager.ResourceRepository.Get(r => r.Name == "Resource1");
        Assert.IsNotNull(resource);
        Assert.AreEqual("Resource1", resource.Name);
        Assert.AreEqual("TypeA", resource.Type);
        Assert.AreEqual("Description of Resource1", resource.Description);
    }

    [TestMethod]
    public void Get_ShouldReturnResource_WhenResourceExists()
    {
        var resourceDTO = new ResourceDTO
        {
            Name = "Resource1",
            Type = "TypeA",
            Description = "Description of Resource1"
        };

        _resourceService.AddResource(resourceDTO);

        var addedResource = _repositoryManager.ResourceRepository.Get(r => r.Name == "Resource1");

        var resource = _resourceService.Get(addedResource.Id);

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
        var resourceDTO1 = new ResourceDTO
        {
            Name = "Resource1",
            Type = "TypeA",
            Description = "Description of Resource1"
        };
        var resourceDTO2 = new ResourceDTO
        {
            Name = "Resource2",
            Type = "TypeB",
            Description = "Description of Resource2"
        };

        _resourceService.AddResource(resourceDTO1);
        _resourceService.AddResource(resourceDTO2);

        var resources = _resourceService.GetResources();

        Assert.AreEqual(2, resources.Count);
        Assert.IsTrue(resources.Exists(r => r.Name == "Resource1"));
        Assert.IsTrue(resources.Exists(r => r.Name == "Resource2"));
    }

    [TestMethod]
    public void UpdateResource_ShouldUpdateResource_WhenResourceExists()
    {
        _loginService.LoginUser("adminSystem.user@example.com", "AdminPassword123@");
        var resourceDTO = new ResourceDTO
        {
            Name = "Resource1",
            Type = "TypeA",
            Description = "Description of Resource1"
        };

        _resourceService.AddResource(resourceDTO);

        var addedResource = _repositoryManager.ResourceRepository.Get(r => r.Name == "Resource1");

        var updatedResourceDTO = new ResourceDTO
        {
            Name = "Resource1",
            Type = "TypeB",
            Description = "Updated description"
        };

        _resourceService.UpdateResource(addedResource.Id, updatedResourceDTO);

        var resource = _repositoryManager.ResourceRepository.Get(r =>
            r.Name == updatedResourceDTO.Name &&
            r.Type == updatedResourceDTO.Type &&
            r.Description == updatedResourceDTO.Description);

        Assert.IsNotNull(resource);
        Assert.AreEqual("TypeB", resource.Type);
        Assert.AreEqual("Updated description", resource.Description);
    }

    [TestMethod]
    public void UpdateResource_ShouldUpdateResource_WhenResourceIsExclusive()
    {
        _loginService.LoginUser("adminSystem.user@example.com", "AdminPassword123@");
        var resourceDTO = new ResourceDTO
        {
            Name = "Resource1",
            Type = "TypeA",
            Description = "Description of Resource1"
        };

        _resourceService.AddResource(resourceDTO);
        _loginService.LoginUser("adminProject.user@example.com", "AdminPassword123@");
        var addedResource = _repositoryManager.ResourceRepository.Get(r => r.Name == "Resource1");

        var addedResourceDto = new ResourceDTO
        {
            Id = addedResource.Id,
            Name = addedResource.Name,
            Type = addedResource.Type,
            Description = addedResource.Description
        };

        var project = new ProjectDTO
        {
            Name = "Project1",
            Description = "Description of Project1",
            StartDate = DateTime.Today,
            AdminProyect = _userService.GetUser("adminProject.user@example.com")
        };

        var task = new TaskDTO
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

        var updatedResourceDTO = new ResourceDTO
        {
            Name = "Resource1",
            Type = "TypeB",
            Description = "Updated description"
        };

        _resourceService.UpdateResource(addedResource.Id, updatedResourceDTO);

        var resource = _repositoryManager.ResourceRepository.Get(r =>
            r.Name == updatedResourceDTO.Name &&
            r.Type == updatedResourceDTO.Type &&
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

        var resourceDTO = new ResourceDTO
        {
            Name = "Resource1",
            Type = "TypeA",
            Description = "Description of Resource1"
        };

        _resourceService.AddResource(resourceDTO);

        _loginService.Logout();
        _loginService.LoginUser("adminProject.user@example.com", "AdminPassword123@");
        var addedResource = _repositoryManager.ResourceRepository.Get(r => r.Name == "Resource1");

        var addedResourceDto = _resourceService.Get(addedResource.Id);

        var project = new ProjectDTO
        {
            Name = "Project 1",
            Description = "Description of Project1",
            StartDate = DateTime.Today
        };

        var project2 = new ProjectDTO
        {
            Name = "Project 2",
            Description = "Description of Project2",
            StartDate = DateTime.Today
        };

        var task = new TaskDTO
        {
            Title = "Title 1",
            Description = "Description1",
            ExpectedStartDate = DateTime.Today,
            Duration = 5,
            PreviousTasks = new List<TaskDTO>(),
            SameTimeTasks = new List<TaskDTO>(),
            Resources = new List<ResourceDTO> { addedResourceDto }
        };

        var task2 = new TaskDTO
        {
            Title = "Title 2",
            Description = "Description2",
            ExpectedStartDate = DateTime.Today.AddDays(10),
            Duration = 5,
            PreviousTasks = new List<TaskDTO>(),
            SameTimeTasks = new List<TaskDTO>(),
            Resources = new List<ResourceDTO> { addedResourceDto }
        };

        _adminProjectService.CreateProject(project);
        _taskService.AddTask("Project 1", task);
        _adminProjectService.CreateProject(project2);
        _taskService.AddTask("Project 2", task2);

        var updatedResourceDTO = new ResourceDTO
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
        var resourceDTO = new ResourceDTO
        {
            Name = "Resource1",
            Type = "TypeA",
            Description = "Description of Resource1"
        };

        _resourceService.AddResource(resourceDTO);
        _loginService.LoginUser("adminProject.user@example.com", "AdminPassword123@");
        var addedResource = _repositoryManager.ResourceRepository.Get(r => r.Name == "Resource1");
        var addedResourceDto = new ResourceDTO
        {
            Id = addedResource.Id,
            Name = addedResource.Name,
            Type = addedResource.Type,
            Description = addedResource.Description
        };

        var project = new ProjectDTO
        {
            Name = "Project1",
            Description = "Description of Project1",
            StartDate = DateTime.Today,
            AdminProyect = _userService.GetUser("adminProject.user@example.com")
        };

        var task = new TaskDTO
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

        var updatedResourceDTO = new ResourceDTO
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
        var resourceDTO = new ResourceDTO
        {
            Name = "ResourceToDelete",
            Type = "TypeA",
            Description = "Description of resource to delete"
        };

        _resourceService.AddResource(resourceDTO);

        var addedResource = _repositoryManager.ResourceRepository.Get(r => r.Name == resourceDTO.Name);
        Assert.IsNotNull(addedResource);

        _resourceService.DeleteResource(addedResource.Id);

        var deletedResource = _repositoryManager.ResourceRepository.Get(r => r.Name == resourceDTO.Name);
        Assert.IsNull(deletedResource);
    }

    [TestMethod]
    public void DeleteResource_ShouldDeleteResource_WhenResourceIsExclusive()
    {
        _loginService.LoginUser("adminSystem.user@example.com", "AdminPassword123@");
        var resourceDTO = new ResourceDTO
        {
            Name = "Resource1",
            Type = "TypeA",
            Description = "Description of Resource1"
        };

        _resourceService.AddResource(resourceDTO);
        _context.ChangeTracker.Clear();

        _loginService.LoginUser("adminProject.user@example.com", "AdminPassword123@");
        var addedResource = _repositoryManager.ResourceRepository.Get(r => r.Name == "Resource1");

        var project = new ProjectDTO
        {
            Name = "Project1",
            Description = "Description of Project1",
            StartDate = DateTime.Today,
            AdminProyect = _userService.GetUser("adminProject.user@example.com")
        };

        _adminProjectService.CreateProject(project);

        var task = new TaskDTO
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

        var projectEntity = _repositoryManager.ProjectRepository.Get(p => p.Name == "Project1");
        var taskEntity = projectEntity.Tasks.First(t => t.Title == "Title1");
        var resourceEntity = _repositoryManager.ResourceRepository.Get(r => r.Id == addedResource.Id);

        taskEntity.Resources.Add(resourceEntity);
        _context.SaveChanges();

        _resourceService.DeleteResource(addedResource.Id);

        var deletedResource = _repositoryManager.ResourceRepository.Get(r => r.Name == resourceDTO.Name);
        Assert.IsNull(deletedResource);
    }

    [TestMethod]
    [ExpectedException(typeof(UnauthorizedAdminAccessException))]
    public void DeleteResource_ShouldThrowException_WhenResourceIsNotExclusive()
    {
        _loginService.Logout();
        _loginService.LoginUser("adminProject.user@example.com", "AdminPassword123@");

        var resourceDTO = new ResourceDTO
        {
            Name = "Resource1",
            Type = "TypeA",
            Description = "Description of Resource1"
        };

        _resourceService.AddResource(resourceDTO);
        _loginService.Logout();
        _loginService.LoginUser("john.doe@example.com", "Password123@");
        var addedResource = _repositoryManager.ResourceRepository.Get(r => r.Name == "Resource1");

        var addedResourceDto = _resourceService.Get(addedResource.Id);

        var project = new ProjectDTO
        {
            Name = "Project 1",
            Description = "Description of Project1",
            StartDate = DateTime.Today
        };

        var project2 = new ProjectDTO
        {
            Name = "Project 2",
            Description = "Description of Project2",
            StartDate = DateTime.Today
        };

        var task = new TaskDTO
        {
            Title = "Title 1",
            Description = "Description1",
            ExpectedStartDate = DateTime.Today,
            Duration = 5,
            PreviousTasks = new List<TaskDTO>(),
            SameTimeTasks = new List<TaskDTO>(),
            Resources = new List<ResourceDTO> { addedResourceDto }
        };

        var task2 = new TaskDTO
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
        var resourceDTO = new ResourceDTO
        {
            Name = "Resource1",
            Type = "TypeA",
            Description = "Description of Resource1"
        };

        _resourceService.AddResource(resourceDTO);

        _loginService.LoginUser("adminProject.user@example.com", "AdminPassword123@");
        var addedResource = _repositoryManager.ResourceRepository.Get(r => r.Name == "Resource1");

        var addedResourceDto = new ResourceDTO
        {
            Id = addedResource.Id,
            Name = addedResource.Name,
            Type = addedResource.Type,
            Description = addedResource.Description
        };

        var project = new ProjectDTO
        {
            Name = "Project1",
            Description = "Description of Project1",
            StartDate = DateTime.Today,
            AdminProyect = _userService.GetUser("adminProject.user@example.com")
        };

        var task = new TaskDTO
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

    [TestMethod]
    public void IsAvailable_ReturnsTrue_WhenResourceAllowConcurrentUsage()
    {
        var resource = new ResourceDTO
        {
            Name = "ConcurrentResource",
            Type = "TypeA",
            Description = "Desc",
            ConcurrentUsage = true
        };
        var available = _resourceService.IsAvailable(resource, DateTime.Today, 5);
        Assert.IsTrue(available);
    }

    [TestMethod]
    public void IsAvailable_ReturnsTrue_WhenNoAssignmentsExist()
    {
        var resource = new ResourceDTO
        {
            Name = "ExclusiveResource",
            Type = "TypeB",
            Description = "Desc",
            ConcurrentUsage = false
        };
        var available = _resourceService.IsAvailable(resource, DateTime.Today, 5);
        Assert.IsTrue(available);
    }

    [TestMethod]
    public void IsAvailable_ReturnsTrue_WhenIntervalDoesNotOverlap()
    {
        _loginService.LoginUser("adminSystem.user@example.com", "AdminPassword123@");
        var resourceDTO = new ResourceDTO
        {
            Name = "Resource1",
            Type = "TypeA",
            Description = "Description of Resource1"
        };
        _resourceService.AddResource(resourceDTO);
        var addedResource = _repositoryManager.ResourceRepository.Get(r => r.Name == "Resource1");
        var addedResourceDto = new ResourceDTO
        {
            Id = addedResource.Id,
            Name = addedResource.Name,
            Type = addedResource.Type,
            Description = addedResource.Description
        };
        _loginService.LoginUser("adminProject.user@example.com", "AdminPassword123@");
        var project = new ProjectDTO
        {
            Name = "Project1",
            Description = "Description of Project1",
            StartDate = DateTime.Today,
            AdminProyect = _userService.GetUser("adminProject.user@example.com")
        };

        var task = new TaskDTO
        {
            Title = "Title1",
            Description = "Description1",
            ExpectedStartDate = DateTime.Today,
            Duration = 5,
            PreviousTasks = new List<TaskDTO>(),
            SameTimeTasks = new List<TaskDTO>(),
            Resources = new List<ResourceDTO> { addedResourceDto }
        };

        var task2 = new TaskDTO
        {
            Title = "Title2",
            Description = "Description2",
            ExpectedStartDate = DateTime.Today.AddDays(10),
            Duration = 5,
            PreviousTasks = new List<TaskDTO>(),
            SameTimeTasks = new List<TaskDTO>(),
            Resources = new List<ResourceDTO> { addedResourceDto }
        };
        _adminProjectService.CreateProject(project);

        _taskService.AddTask("Project1", task);
        _taskService.AddTask("Project1", task2);

        var available = _resourceService.IsAvailable(addedResourceDto, DateTime.Today.AddDays(6), 3);

        Assert.IsTrue(available);
    }

    [TestMethod]
    public void IsAvailable_ReturnsFalse_WhenIntervalOverlaps()
    {
        _loginService.LoginUser("adminSystem.user@example.com", "AdminPassword123@");
        var resourceDTO = new ResourceDTO
        {
            Name = "Resource1",
            Type = "TypeA",
            Description = "Description of Resource1"
        };
        _resourceService.AddResource(resourceDTO);
        var addedResource = _repositoryManager.ResourceRepository.Get(r => r.Name == "Resource1");
        var addedResourceDto = new ResourceDTO
        {
            Id = addedResource.Id,
            Name = addedResource.Name,
            Type = addedResource.Type,
            Description = addedResource.Description
        };
        _loginService.LoginUser("adminProject.user@example.com", "AdminPassword123@");
        var project = new ProjectDTO
        {
            Name = "Project1",
            Description = "Description of Project1",
            StartDate = DateTime.Today,
            AdminProyect = _userService.GetUser("adminProject.user@example.com")
        };

        var task = new TaskDTO
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

        var available = _resourceService.IsAvailable(addedResourceDto, DateTime.Today, 3);

        Assert.IsFalse(available);
    }

    [TestMethod]
    public void NextDateAvailable_ReturnsStartDate_WhenResourceAllowConcurrentUsage()
    {
        var resource = new ResourceDTO
        {
            Name = "ConcurrentResource",
            Type = "TypeA",
            Description = "Desc",
            ConcurrentUsage = true
        };
        var today = DateTime.Today;
        var next = _resourceService.NextDateAvailable(resource, today, 5);
        Assert.AreEqual(today, next);
    }

    [TestMethod]
    public void NextDateAvailable_MovesToEndOfBlockingAssignment_WhenAssignmentOverlapsStartDate()
    {
        _loginService.LoginUser("adminSystem.user@example.com", "AdminPassword123@");
        var resourceDTO = new ResourceDTO
        {
            Name = "Resource1",
            Type = "TypeA",
            Description = "Description of Resource1"
        };
        _resourceService.AddResource(resourceDTO);
        var added = _repositoryManager.ResourceRepository.Get(r => r.Name == "Resource1");
        var resDto = _resourceService.Get(added.Id);

        _loginService.LoginUser("adminProject.user@example.com", "AdminPassword123@");
        var project = new ProjectDTO
        {
            Name = "Project1",
            Description = "Description of Project1",
            StartDate = DateTime.Today,
            AdminProyect = _userService.GetUser("adminProject.user@example.com")
        };
        _adminProjectService.CreateProject(project);
        var task = new TaskDTO
        {
            Title = "T1",
            Description = "Desc",
            ExpectedStartDate = DateTime.Today,
            Duration = 5,
            PreviousTasks = new List<TaskDTO>(),
            SameTimeTasks = new List<TaskDTO>(),
            Resources = new List<ResourceDTO> { resDto }
        };
        _taskService.AddTask("Project1", task);
        var next = _resourceService.NextDateAvailable(resDto, DateTime.Today, 3);
        Assert.AreEqual(DateTime.Today.AddDays(5), next);
    }

    [TestMethod]
    public void NextDateAvailable_FindsFirstGapBetweenAssignments_WhenGapFits()
    {
        _loginService.LoginUser("adminSystem.user@example.com", "AdminPassword123@");
        var resourceDTO = new ResourceDTO
        {
            Name = "Resource2",
            Type = "TypeA",
            Description = "Description of Resource2"
        };
        _resourceService.AddResource(resourceDTO);
        var added = _repositoryManager.ResourceRepository.Get(r => r.Name == "Resource2");
        var resDto = _resourceService.Get(added.Id);

        _loginService.LoginUser("adminProject.user@example.com", "AdminPassword123@");
        var project = new ProjectDTO
        {
            Name = "Project2",
            Description = "Description of Project2",
            StartDate = DateTime.Today,
            AdminProyect = _userService.GetUser("adminProject.user@example.com")
        };
        _adminProjectService.CreateProject(project);

        var t1 = new TaskDTO
        {
            Title = "T1",
            Description = "Desc1",
            ExpectedStartDate = DateTime.Today,
            Duration = 2,
            PreviousTasks = new List<TaskDTO>(),
            SameTimeTasks = new List<TaskDTO>(),
            Resources = new List<ResourceDTO> { resDto }
        };
        var t2 = new TaskDTO
        {
            Title = "T2",
            Description = "Desc2",
            ExpectedStartDate = DateTime.Today.AddDays(4),
            Duration = 2,
            PreviousTasks = new List<TaskDTO>(),
            SameTimeTasks = new List<TaskDTO>(),
            Resources = new List<ResourceDTO> { resDto }
        };
        _taskService.AddTask("Project2", t1);
        _taskService.AddTask("Project2", t2);

        var next = _resourceService.NextDateAvailable(resDto, DateTime.Today, 2);
        Assert.AreEqual(DateTime.Today.AddDays(2), next);
    }

    [TestMethod]
    public void NextDateAvailable_ReturnsFirstDateAfterLastAssignment_WhenNoGapFits()
    {
        _loginService.LoginUser("adminSystem.user@example.com", "AdminPassword123@");
        var resourceDTO = new ResourceDTO
        {
            Name = "Resource3",
            Type = "TypeA",
            Description = "Description of Resource3"
        };
        _resourceService.AddResource(resourceDTO);
        var added = _repositoryManager.ResourceRepository.Get(r => r.Name == "Resource3");
        var resDto = _resourceService.Get(added.Id);

        _loginService.LoginUser("adminProject.user@example.com", "AdminPassword123@");
        var project = new ProjectDTO
        {
            Name = "Project3",
            Description = "Description of Project3",
            StartDate = DateTime.Today,
            AdminProyect = _userService.GetUser("adminProject.user@example.com")
        };
        _adminProjectService.CreateProject(project);

        var t1 = new TaskDTO
        {
            Title = "T1",
            Description = "Desc1",
            ExpectedStartDate = DateTime.Today,
            Duration = 2,
            PreviousTasks = new List<TaskDTO>(),
            SameTimeTasks = new List<TaskDTO>(),
            Resources = new List<ResourceDTO> { resDto }
        };
        var t2 = new TaskDTO
        {
            Title = "T2",
            Description = "Desc2",
            ExpectedStartDate = DateTime.Today.AddDays(3),
            Duration = 5,
            PreviousTasks = new List<TaskDTO>(),
            SameTimeTasks = new List<TaskDTO>(),
            Resources = new List<ResourceDTO> { resDto }
        };
        _taskService.AddTask("Project3", t1);
        _taskService.AddTask("Project3", t2);
        var next = _resourceService.NextDateAvailable(resDto, DateTime.Today, 3);
        Assert.AreEqual(DateTime.Today.AddDays(8), next);
    }

    [TestMethod]
    public void GetAllResourcesForAProject_ReturnsAllUniqueResources_WhenTasksHaveDistinctResources()
    {
        _loginService.LoginUser("adminProject.user@example.com", "AdminPassword123@");
        var projectDto = new ProjectDTO
        {
            Name = "ProjDistinct",
            Description = "Two tasks, two resources",
            StartDate = DateTime.Today,
            AdminProyect = _userService.GetUser("adminProject.user@example.com")
        };
        _adminProjectService.CreateProject(projectDto);
        _loginService.LoginUser("adminSystem.user@example.com", "AdminPassword123@");
        var r1 = new ResourceDTO { Name = "R1", Type = "T1", Description = "D1" };
        var r2 = new ResourceDTO { Name = "R2", Type = "T2", Description = "D2" };
        _resourceService.AddResource(r1);
        _resourceService.AddResource(r2);
        r1.Id = _repositoryManager.ResourceRepository.Get(r => r.Name == "R1").Id;
        r2.Id = _repositoryManager.ResourceRepository.Get(r => r.Name == "R2").Id;
        var taskA = new TaskDTO
        {
            Title = "TaskA",
            Description = "uses R1",
            ExpectedStartDate = DateTime.Today.AddDays(1),
            Duration = 2,
            Resources = new List<ResourceDTO> { r1 }
        };
        _taskService.AddTask("ProjDistinct", taskA);
        var taskB = new TaskDTO
        {
            Title = "TaskB",
            Description = "uses R2",
            ExpectedStartDate = DateTime.Today.AddDays(3),
            Duration = 2,
            Resources = new List<ResourceDTO> { r2 }
        };
        _taskService.AddTask("ProjDistinct", taskB);
        var resources = _resourceService.getAllResourcesForAProject("ProjDistinct");
        Assert.AreEqual(2, resources.Count);
        Assert.IsTrue(resources.Any(r => r.Name == "R1"));
        Assert.IsTrue(resources.Any(r => r.Name == "R2"));
    }

    [TestMethod]
    public void GetAllResourcesForAProject_ReturnsDistinct_WhenMultipleTasksShareSameResource()
    {
        _loginService.LoginUser("adminProject.user@example.com", "AdminPassword123@");
        var projectDto = new ProjectDTO
        {
            Name = "ProjShared",
            Description = "Two tasks share R1",
            StartDate = DateTime.Today,
            AdminProyect = _userService.GetUser("adminProject.user@example.com")
        };
        _adminProjectService.CreateProject(projectDto);
        _loginService.LoginUser("adminSystem.user@example.com", "AdminPassword123@");
        var r1 = new ResourceDTO { Name = "SharedR", Type = "T", Description = "D" };
        _resourceService.AddResource(r1);
        r1.Id = _repositoryManager.ResourceRepository.Get(r => r.Name == "SharedR").Id;
        var task1 = new TaskDTO
        {
            Title = "T1",
            Description = "uses SharedR",
            ExpectedStartDate = DateTime.Today.AddDays(1),
            Duration = 2,
            Resources = new List<ResourceDTO> { r1 }
        };
        _taskService.AddTask("ProjShared", task1);
        var task2 = new TaskDTO
        {
            Title = "T2",
            Description = "also uses SharedR",
            ExpectedStartDate = DateTime.Today.AddDays(4),
            Duration = 3,
            Resources = new List<ResourceDTO> { r1 }
        };
        _taskService.AddTask("ProjShared", task2);
        var resources = _resourceService.getAllResourcesForAProject("ProjShared");
        Assert.AreEqual(1, resources.Count);
        Assert.AreEqual("SharedR", resources[0].Name);
    }

    [TestMethod]
    public void GetWhenResourceOccupied_ReturnsMultipleEntries_WhenMultipleTasksUseSameResource()
    {
        _loginService.LoginUser("adminProject.user@example.com", "AdminPassword123@");
        var project = new ProjectDTO
        {
            Name = "ProjMultiOcc",
            Description = "Multiple tasks",
            StartDate = DateTime.Today,
            AdminProyect = _userService.GetUser("adminProject.user@example.com")
        };
        _adminProjectService.CreateProject(project);
        _loginService.LoginUser("adminSystem.user@example.com", "AdminPassword123@");
        var r = new ResourceDTO { Name = "ResB", Type = "TB", Description = "DB" };
        _resourceService.AddResource(r);
        r.Id = _repositoryManager.ResourceRepository.Get(x => x.Name == "ResB").Id;
        _loginService.LoginUser("adminProject.user@example.com", "AdminPassword123@");
        var t1 = new TaskDTO
        {
            Title = "T1",
            Description = "first use",
            ExpectedStartDate = DateTime.Today,
            Duration = 2,
            Resources = new List<ResourceDTO> { r }
        };
        _taskService.AddTask("ProjMultiOcc", t1);
        var t2 = new TaskDTO
        {
            Title = "T2",
            Description = "second use",
            ExpectedStartDate = DateTime.Today.AddDays(5),
            Duration = 3,
            Resources = new List<ResourceDTO> { r }
        };
        _taskService.AddTask("ProjMultiOcc", t2);
        var occupied = _resourceService.getWhenIsResourceOcupied(r);
        Assert.AreEqual(2, occupied.Count);
        Assert.IsTrue(occupied.Any(o => o.Item1 == DateTime.Today && o.Item2 == 2));
        Assert.IsTrue(occupied.Any(o => o.Item1 == DateTime.Today.AddDays(5) && o.Item2 == 3));
    }

    [TestMethod]
    public void UpdateResourceDependencies_ShouldAddPreviousTasks_BasedOnResourceAndStartDate()
    {
        _loginService.LoginUser("adminProject.user@example.com", "AdminPassword123@");
        var projectDto = new ProjectDTO
        {
            Name = "ProjDeps",
            Description = "Proyecto para dependencias",
            StartDate = DateTime.Today,
            AdminProyect = _userService.GetUser("adminProject.user@example.com")
        };
        _adminProjectService.CreateProject(projectDto);

        _loginService.Logout();
        _loginService.LoginUser("adminSystem.user@example.com", "AdminPassword123@");
        var resDto = new ResourceDTO { Name = "R", Type = "T", Description = "D" };
        _resourceService.AddResource(resDto);
        var resEntity = _repositoryManager.ResourceRepository.Get(r => r.Name == "R");
        resDto.Id = resEntity.Id;

        _loginService.Logout();
        _loginService.LoginUser("adminProject.user@example.com", "AdminPassword123@");

        var tA = new TaskDTO
        {
            Title = "A",
            Description = "usa R",
            ExpectedStartDate = DateTime.Today,
            Duration = 1,
            Resources = new List<ResourceDTO> { resDto }
        };
        _taskService.AddTask("ProjDeps", tA);

        var tB = new TaskDTO
        {
            Title = "B",
            Description = "usa R",
            ExpectedStartDate = DateTime.Today.AddDays(1),
            Duration = 1,
            Resources = new List<ResourceDTO> { resDto }
        };
        _taskService.AddTask("ProjDeps", tB);

        var tC = new TaskDTO
        {
            Title = "C",
            Description = "usa R",
            ExpectedStartDate = DateTime.Today.AddDays(2),
            Duration = 1,
            Resources = new List<ResourceDTO> { resDto }
        };
        _taskService.AddTask("ProjDeps", tC);

        var createdC = _taskService.GetTask("ProjDeps", "C");

        var result = _resourceService.updateResourceDependencies(createdC, "ProjDeps");
        Assert.AreEqual(2, result.PreviousTasks.Count);
        List<string> titles = result.PreviousTasks.Select(p => p.Title).ToList();
        CollectionAssert.AreEquivalent(new[] { "A", "B" }, titles);
    }

    [TestMethod]
    [ExpectedException(typeof(ResourceNotFoundException))]
    public void UpdateResource_ShouldThrowResourceNotFoundException_WhenResourceDoesNotExist()
    {
        _loginService.LoginUser("adminSystem.user@example.com", "AdminPassword123@");

        var nonExistentResource = new ResourceDTO
        {
            Name = "Non Existent Resource",
            Type = "TypeX",
            Description = "This resource does not exist"
        };

        _resourceService.UpdateResource(999, nonExistentResource);
    }

    [TestMethod]
    public void GetWhenResourceOccupied_ShouldReturnEmptyList_WhenResourceDoesNotExist()
    {
        var nonExistentResource = new ResourceDTO
        {
            Id = 999,
            Name = "Non Existent",
            Type = "Type",
            Description = "Description"
        };

        var result = _resourceService.getWhenIsResourceOcupied(nonExistentResource);

        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void GetResourcesForAProject_ShouldReturnAllResourcesForTheProject_WhenResourcesExist()
    {
        _loginService.LoginUser("adminProject.user@example.com", "AdminPassword123@");
        var projectDTO1 = new ProjectDTO
        {
            Name = "Project 1",
            Description = "Description 1",
            StartDate = DateTime.Now
        };
        var resourceDTO1 = new ResourceDTO
        {
            Name = "Resource1",
            Type = "TypeA",
            Description = "Description of Resource1",
            Project = projectDTO1
        };

        var resourceDTO2 = new ResourceDTO
        {
            Name = "Resource2",
            Type = "TypeB",
            Description = "Description of Resource2"
        };
        _adminProjectService.CreateProject(projectDTO1);

        _resourceService.AddResource(resourceDTO1);
        _resourceService.AddResource(resourceDTO2);

        var resources = _resourceService.GetResourcesForProject("Project 1");

        Assert.AreEqual(2, resources.Count);
        Assert.IsTrue(resources.Exists(r => r.Name == "Resource1"));
        Assert.IsTrue(resources.Exists(r => r.Name == "Resource2"));
    }
}