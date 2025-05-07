using DataAccess;
using DataAccess.ResourceRepositoryExceptions;
using Domain;
using Domain.Exceptions;
using Service.Models;
using Task = Domain.Task;

namespace Service.Test;

[TestClass]
public class ResourcesServiceTest
{
    private InMemoryDatabase _database;
    private ResourceService _resourceService;
    private Login _loginService;
    private UserService _userService;
    private AdminPService _adminProjectService;
    private TaskService _taskService;
    [TestInitialize]
    public void TestSetUp()
    {
        _database = new InMemoryDatabase();
        _loginService = new Login(_database);
        _userService = new UserService(_database);
        _resourceService = new ResourceService(_database);
        _adminProjectService = new AdminPService(_database);
        _taskService = new TaskService(_database);

        var adminSUserDTO = new UserDTO
        {
            FirstName = "AdminSystem",
            LastName = "User",
            Email = "adminSystem.user@example.com",
            Password = "AdminPassword123@",
            Roles = new List<Rol> { Rol.AdminSystem }
        };
        var adminPUserDTO = new UserDTO
        {
            FirstName = "AdminProject",
            LastName = "User",
            Email = "adminProject.user@example.com",
            Password = "AdminPassword123@",
            Roles = new List<Rol> { Rol.AdminProject }
        };
        
        
        var adminPUserDTO2 = new UserDTO
        {
            FirstName = "AdminProject2",
            LastName = "User2",
            Email = "adminProject2.user@example.com",
            Password = "AdminPassword123@",
            Roles = new List<Rol> { Rol.AdminProject }
        };

        var normalUserDTO = new UserDTO
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Password = "Password123@",
            Roles = new List<Rol>()
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

        var resource = _database.resources.Get(r => r.Name == "Resource1");
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

        var addedResource = _database.resources.Get(r => r.Name == "Resource1");

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
            Description = "Description of Resource1",
        };

        _resourceService.AddResource(resourceDTO);

        var addedResource = _database.resources.Get(r => r.Name == "Resource1");

        var updatedResourceDTO = new ResourceDTO
        {
            Name = "Resource1",
            Type = "TypeB",
            Description = "Updated description"
        };

        _resourceService.UpdateResource(addedResource.Id, updatedResourceDTO);

        var resource = _database.resources.Get(r =>
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
        var resourceDTO = new ResourceDTO
        {
            Name = "Resource1",
            Type = "TypeA",
            Description = "Description of Resource1",
        };

        _resourceService.AddResource(resourceDTO);
        _loginService.LoginUser("adminProject.user@example.com", "AdminPassword123@");
        var addedResource = _database.resources.Get(r => r.Name == "Resource1");
        
        ProjectDTO project = new ProjectDTO();
        project.Name = "Project1";
        project.Description = "Description of Project1";
        project.StartDate = DateTime.Today;
        project.AdminProyect = _userService.GetUser( "adminProject.user@example.com");
        
        TaskDTO task = new TaskDTO()
        {
            Title = "Title1", 
            Description = "Description1" , 
            ExpectedStartDate = DateTime.Today, 
            Duration = 5,
            PreviousTasks = new List<Task>(), 
            SameTimeTasks = new List<Task>(),
            Resources = new List<Resource>(){addedResource}
        };
        
        _adminProjectService.CreateProject(project);
        _taskService.AddTask("Project1",task );
       
        var updatedResourceDTO = new ResourceDTO
        {
            Name = "Resource1",
            Type = "TypeB",
            Description = "Updated description"
        };
        
        _resourceService.UpdateResource(addedResource.Id, updatedResourceDTO);

        var resource = _database.resources.Get(r =>
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
        _loginService.LoginUser("adminSystem.user@example.com", "AdminPassword123@");
        var resourceDTO = new ResourceDTO
        {
            Name = "Resource1",
            Type = "TypeA",
            Description = "Description of Resource1",
        };

        _resourceService.AddResource(resourceDTO);
        _loginService.LoginUser("adminProject.user@example.com", "AdminPassword123@");
        var addedResource = _database.resources.Get(r => r.Name == "Resource1");
        
        ProjectDTO project = new ProjectDTO();
        project.Name = "Project 1";
        project.Description = "Description of Project1";
        project.StartDate = DateTime.Today;
        project.AdminProyect = _userService.GetUser( "adminProject.user@example.com");
        
        ProjectDTO project2 = new ProjectDTO();
        project2.Name = "Project 2";
        project2.Description = "Description of Project2";
        project2.StartDate = DateTime.Today;
        project2.AdminProyect = _userService.GetUser( "adminProject.user@example.com");
        
        TaskDTO task = new TaskDTO()
        {
            Title = "Title 1", 
            Description = "Description1" , 
            ExpectedStartDate = DateTime.Today, 
            Duration = 5,
            PreviousTasks = new List<Task>(), 
            SameTimeTasks = new List<Task>(),
            Resources = new List<Resource>(){addedResource}
        };
        TaskDTO task2 = new TaskDTO()
        {
            Title = "Title 2", 
            Description = "Description2" , 
            ExpectedStartDate = DateTime.Today, 
            Duration = 5,
            PreviousTasks = new List<Task>(), 
            SameTimeTasks = new List<Task>(),
            Resources = new List<Resource>(){addedResource}
        };
        
        _adminProjectService.CreateProject(project);
        _taskService.AddTask("Project 1",task );
        _adminProjectService.CreateProject(project2);
        _taskService.AddTask("Project 2",task2 );
       
        var updatedResourceDTO = new ResourceDTO
        {
            Name = "Resource1",
            Type = "TypeB",
            Description = "Updated description"
        };
        
        _resourceService.UpdateResource(addedResource.Id, updatedResourceDTO);
        
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
            Description = "Description of Resource1",
        };

        _resourceService.AddResource(resourceDTO);
        _loginService.LoginUser("adminProject.user@example.com", "AdminPassword123@");
        var addedResource = _database.resources.Get(r => r.Name == "Resource1");
        
        ProjectDTO project = new ProjectDTO();
        project.Name = "Project1";
        project.Description = "Description of Project1";
        project.StartDate = DateTime.Today;
        project.AdminProyect = _userService.GetUser( "adminProject.user@example.com");
        
        TaskDTO task = new TaskDTO()
        {
            Title = "Title1", 
            Description = "Description1" , 
            ExpectedStartDate = DateTime.Today, 
            Duration = 5,
            PreviousTasks = new List<Task>(), 
            SameTimeTasks = new List<Task>(),
            Resources = new List<Resource>(){addedResource}
        };
        
        _adminProjectService.CreateProject(project);
        _taskService.AddTask("Project1",task );
       
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

        var addedResource = _database.resources.Get(r => r.Name == resourceDTO.Name);
        Assert.IsNotNull(addedResource);

        _resourceService.DeleteResource(addedResource.Id);

        var deletedResource = _database.resources.Get(r => r.Name == resourceDTO.Name);
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
            Description = "Description of Resource1",
        };

        _resourceService.AddResource(resourceDTO);
        _loginService.LoginUser("adminProject.user@example.com", "AdminPassword123@");
        var addedResource = _database.resources.Get(r => r.Name == "Resource1");
        
        ProjectDTO project = new ProjectDTO();
        project.Name = "Project1";
        project.Description = "Description of Project1";
        project.StartDate = DateTime.Today;
        project.AdminProyect = _userService.GetUser( "adminProject.user@example.com");
        
        TaskDTO task = new TaskDTO()
        {
            Title = "Title1", 
            Description = "Description1" , 
            ExpectedStartDate = DateTime.Today, 
            Duration = 5,
            PreviousTasks = new List<Task>(), 
            SameTimeTasks = new List<Task>(),
            Resources = new List<Resource>(){addedResource}
        };
        
        _adminProjectService.CreateProject(project);
        _taskService.AddTask("Project1",task );
        
        Assert.IsNotNull(addedResource);

        _resourceService.DeleteResource(addedResource.Id);

        var deletedResource = _database.resources.Get(r => r.Name == resourceDTO.Name);
        Assert.IsNull(deletedResource);
    }
    [TestMethod]
    [ExpectedException(typeof(UnauthorizedAdminAccessException))]
    public void DeleteResource_ShouldThrowException_WhenResourceIsNotExclusive()
    {
        _loginService.LoginUser("adminSystem.user@example.com", "AdminPassword123@");
        var resourceDTO = new ResourceDTO
        {
            Name = "Resource1",
            Type = "TypeA",
            Description = "Description of Resource1",
        };

        _resourceService.AddResource(resourceDTO);
        _loginService.LoginUser("adminProject.user@example.com", "AdminPassword123@");
        var addedResource = _database.resources.Get(r => r.Name == "Resource1");
        
        ProjectDTO project = new ProjectDTO();
        project.Name = "Project 1";
        project.Description = "Description of Project1";
        project.StartDate = DateTime.Today;
        project.AdminProyect = _userService.GetUser( "adminProject.user@example.com");
        
        ProjectDTO project2 = new ProjectDTO();
        project2.Name = "Project 2";
        project2.Description = "Description of Project2";
        project2.StartDate = DateTime.Today;
        project2.AdminProyect = _userService.GetUser( "adminProject.user@example.com");
        
        TaskDTO task = new TaskDTO()
        {
            Title = "Title 1", 
            Description = "Description1" , 
            ExpectedStartDate = DateTime.Today, 
            Duration = 5,
            PreviousTasks = new List<Task>(), 
            SameTimeTasks = new List<Task>(),
            Resources = new List<Resource>(){addedResource}
        };
        TaskDTO task2 = new TaskDTO()
        {
            Title = "Title 2", 
            Description = "Description2" , 
            ExpectedStartDate = DateTime.Today, 
            Duration = 5,
            PreviousTasks = new List<Task>(), 
            SameTimeTasks = new List<Task>(),
            Resources = new List<Resource>(){addedResource}
        };
        
        _adminProjectService.CreateProject(project);
        _taskService.AddTask("Project 1",task );
        _adminProjectService.CreateProject(project2);
        _taskService.AddTask("Project 2",task2 );
        Assert.IsNotNull(addedResource);

        _resourceService.DeleteResource(addedResource.Id);
        
    }

    [TestMethod]
    [ExpectedException(typeof(ResourceNotFoundException))]
    public void DeleteResource_ShouldThroException_WhenResourceNoExists()
    {
        _resourceService.DeleteResource(999);
    }
}