using DataAccess;
using DataAccess.Exceptions.ProjectRepositoryExceptions;
using Domain;
using Service.Converters;
using Service.Exceptions.AdminPServiceExceptions;
using Service.Models;
using Task = System.Threading.Tasks.Task;

namespace Service.Test;

[TestClass]
public class AdminPServiceTests
{
    private AppDbContext _context;
    private IRepositoryManager _repositoryManager;
    private Login _login;
    private AdminPService _adminPservice;
    private TaskService _taskService;
    private UserService _userservice;
    private UserDTO Admin;
    private List<UserDTO> members;
    private UserDTO UserDTO;
    private UserDTO Leader;

    [TestInitialize]
    public void Setup()
    {
        InMemoryAppContextFactory contextFactory = new InMemoryAppContextFactory();
        _context = contextFactory.CreateDbContext();

        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();

        _repositoryManager = new RepositoryManager(_context);

        CpmService cpmService = new CpmService();
        _taskService = new TaskService(_repositoryManager, cpmService);
        _adminPservice = new AdminPService(_repositoryManager);

        _userservice = new UserService(_repositoryManager);
        _login = new Login(_repositoryManager);

        Admin = new UserDTO
        {
            FirstName = "Admin",
            LastName = "User",
            Email = "admin.user@example.com",
            Birthday = DateTime.Parse("1990-01-01"),
            Password = "Password123@",
            Roles = new List<RolDTO> { RolDTO.AdminProject }
        };

        UserDTO = new UserDTO
        {
            FirstName = "User",
            LastName = "Member",
            Email = "member.user@example.com",
            Birthday = DateTime.Parse("1990-01-01"),
            Password = "Password123@",
            Roles = new List<RolDTO> { RolDTO.ProjectMember }
        };

        Leader = new UserDTO()
        {
            FirstName = "Normal",
            LastName = "User",
            Email = "normal.user@example.com",
            Birthday = DateTime.Parse("1990-01-01"),
            Password = "Password123@",
            Roles = new List<RolDTO> { RolDTO.ProjectLeader }
        };

        members = new List<UserDTO> { UserDTO };

        _userservice.AddUser(Admin);
        _userservice.AddUser(UserDTO);
        _userservice.AddUser(Leader);
        _login.LoginUser(Admin.Email, Admin.Password);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _context?.Database.EnsureDeleted();
    }

    [TestMethod]
    public void CreateProject_ShouldAddProjectToDatabase_WhenValid()
    {
        ProjectDTO projectDTO = new ProjectDTO
        {
            Name = "New Project",
            Description = "Project Description",
            StartDate = DateTime.Today,
            Members = members
        };

        _adminPservice.CreateProject(projectDTO);

        Project project = _repositoryManager.ProjectRepository.Get(p => p.Name == projectDTO.Name);
        Assert.IsNotNull(project);
        Assert.AreEqual("New Project", project.Name);
    }

    [TestMethod]
    public void AssignMembersToProject_ShouldAddMembers_WhenValid()
    {
        ProjectDTO projectDTO = new ProjectDTO
        {
            Name = "New Project",
            Description = "Project Description",
            StartDate = DateTime.Now,
            AdminProyect = UserDTO,
            Members = members
        };

        _adminPservice.CreateProject(projectDTO);

        UserDTO newUserDTO = new UserDTO
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Birthday = DateTime.Parse("1990-01-01"),
            Password = "Password123!",
            Roles = new List<RolDTO> { RolDTO.ProjectMember }
        };

        UserService userService = new UserService(_repositoryManager);
        userService.AddUser(newUserDTO);

        Project projectBeforeAdd = _repositoryManager.ProjectRepository.Get(p => p.Name == projectDTO.Name);
        int initialMemberCount = projectBeforeAdd.Members?.Count ?? 0;

        _adminPservice.AssignMembersToProject(projectDTO.Name, new List<UserDTO> { newUserDTO });

        Project projectAfterAdd = _repositoryManager.ProjectRepository.Get(p => p.Name == projectDTO.Name);

        Assert.IsNotNull(projectAfterAdd.Members, "Members list should not be null");
        Assert.AreEqual(initialMemberCount + 1, projectAfterAdd.Members.Count, "Should have one more member");

        User addedUser = projectAfterAdd.Members.FirstOrDefault(m => m.Email == "john.doe@example.com");
        Assert.IsNotNull(addedUser, "John should be found in the members list");
        Assert.AreEqual("John", addedUser.FirstName, "First name should match");
        Assert.AreEqual("Doe", addedUser.LastName, "Last name should match");
    }

    [TestMethod]
    [ExpectedException(typeof(UserIsAlreadyAMemberException))]
    public void AssignMembersToProject_ShouldThrowException_WhenAddingAMemberThatAlreadyExists()
    {
        ProjectDTO projectDTO = new ProjectDTO
        {
            Name = "New Project",
            Description = "Project Description",
            StartDate = DateTime.Now,
            AdminProyect = UserDTO,
            Members = members
        };

        _adminPservice.CreateProject(projectDTO);

        UserDTO userDTO = new UserDTO
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Birthday = DateTime.Parse("1990-01-01"),
            Password = "Password123!",
            Roles = new List<RolDTO> { RolDTO.ProjectMember }
        };

        UserService userService = new UserService(_repositoryManager);
        userService.AddUser(userDTO);

        _adminPservice.AssignMembersToProject(projectDTO.Name, new List<UserDTO> { userDTO });

        Project project = _repositoryManager.ProjectRepository.Get(p => p.Name == projectDTO.Name);
        Assert.IsTrue(project.Members.Any(m => m.Email == "john.doe@example.com"),
            "User should be added to project after first call");

        _adminPservice.AssignMembersToProject(projectDTO.Name, new List<UserDTO> { userDTO });

        Assert.Fail("Expected UserIsAlreadyAMemberException was not thrown");
    }

    [TestMethod]
    public void RemoveMembersFromProject_ShouldRemoveMember_WhenMemberExists()
    {
        ProjectDTO projectDTO = new ProjectDTO
        {
            Name = "New Project",
            Description = "Project Description",
            StartDate = DateTime.Now,
            AdminProyect = UserDTO,
            Members = new List<UserDTO>()
        };

        _adminPservice.CreateProject(projectDTO);

        UserDTO johnDTO = new UserDTO
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "johndoe@example.com",
            Birthday = DateTime.Parse("1990-01-01"),
            Password = "Password123!",
            Roles = new List<RolDTO> { RolDTO.ProjectMember }
        };

        UserDTO memberDTO = new UserDTO
        {
            FirstName = "Member",
            LastName = "User",
            Email = "memberuser@example.com",
            Birthday = DateTime.Parse("1985-05-15"),
            Password = "Password123!",
            Roles = new List<RolDTO> { RolDTO.ProjectMember }
        };

        UserService userService = new UserService(_repositoryManager);
        userService.AddUser(johnDTO);
        userService.AddUser(memberDTO);

        _adminPservice.AssignMembersToProject(projectDTO.Name, new List<UserDTO> { johnDTO, memberDTO });

        Project projectAfterAdd = _repositoryManager.ProjectRepository.Get(p => p.Name == projectDTO.Name);

        Assert.IsTrue(projectAfterAdd.Members.Any(m => m.Email == "johndoe@example.com"),
            "John should be in the project");
        Assert.IsTrue(projectAfterAdd.Members.Any(m => m.Email == "memberuser@example.com"),
            "Member should be in the project");
        Assert.AreEqual(2, projectAfterAdd.Members.Count, "Should have exactly 2 members");

        User johnInProject = projectAfterAdd.Members.First(m => m.Email == "johndoe@example.com");
        Assert.AreEqual("John", johnInProject.FirstName, "John's first name should be correct");

        _adminPservice.RemoveMemberFromProject(projectDTO.Name, "johndoe@example.com");
        _adminPservice.RemoveMemberFromProject(projectDTO.Name, "memberuser@example.com");

        Project projectAfterRemove = _repositoryManager.ProjectRepository.Get(p => p.Name == projectDTO.Name);

        Assert.IsFalse(projectAfterRemove.Members.Any(m => m.Email == "johndoe@example.com"),
            "John should be removed from the project");
        Assert.IsFalse(projectAfterRemove.Members.Any(m => m.Email == "memberuser@example.com"),
            "Member should be removed from the project");
        Assert.AreEqual(0, projectAfterRemove.Members.Count,
            "Project should have no members after removing all");
    }

    [TestMethod]
    [ExpectedException(typeof(ProjectNotFoundException))]
    public void RemoveMembersFromProject_ShouldThrowException_WhenRemovingMemberFromAProjectThatNotExist()
    {
        ProjectDTO projectDTO = new ProjectDTO
        {
            Name = "New Project",
            Description = "Project Description",
            StartDate = DateTime.Now,
            AdminProyect = UserDTO,
            Members = members
        };

        _adminPservice.CreateProject(projectDTO);

        Project project = _repositoryManager.ProjectRepository.Get(p => p.Name == projectDTO.Name);

        _adminPservice.RemoveMemberFromProject("Proyecto 1", "member.user@example.com");
    }

    [TestMethod]
    [ExpectedException(typeof(UserIsNotAMemberException))]
    public void RemoveMembersFromProject_ShouldThrowException_WhenRemovingMemberThatNotExists()
    {
        ProjectDTO projectDTO = new ProjectDTO
        {
            Name = "New Project",
            Description = "Project Description",
            StartDate = DateTime.Now,
            AdminProyect = UserDTO,
            Members = members
        };

        _adminPservice.CreateProject(projectDTO);

        Project project = _repositoryManager.ProjectRepository.Get(p => p.Name == projectDTO.Name);

        _adminPservice.RemoveMemberFromProject(project.Name, "john.user@example.com");
    }

    [TestMethod]
    public void RemoveProject_ShouldRemoveProject_WhenValid()
    {
        ProjectDTO projectDTO = new ProjectDTO
        {
            Name = "New Project",
            Description = "Project Description",
            StartDate = DateTime.Now,
            AdminProyect = UserDTO,
            Members = members
        };

        _adminPservice.CreateProject(projectDTO);

        Project project = _repositoryManager.ProjectRepository.Get(p => p.Name == projectDTO.Name);
        Assert.IsNotNull(project);

        _adminPservice.RemoveProject("New Project");

        project = _repositoryManager.ProjectRepository.Get(p => p.Name == "New Project");
        Assert.IsNull(project);
    }


    [TestMethod]
    public void UpdateProject_ShouldUpdateProject_WhenValid()
    {
        ProjectDTO projectDTO = new ProjectDTO
        {
            Name = "Old Project",
            Description = "Old Description",
            StartDate = DateTime.Now,
            AdminProyect = UserDTO,
            Members = members
        };

        _adminPservice.CreateProject(projectDTO);

        Project project = _repositoryManager.ProjectRepository.Get(p => p.Name == projectDTO.Name);
        Assert.IsNotNull(project);

        ProjectDTO updatedDTO = new ProjectDTO
        {
            Name = "Updated Project",
            Description = "Updated Description",
            StartDate = DateTime.Now.AddDays(1),
            AdminProyect = UserDTO,
            Members = members
        };

        _adminPservice.UpdateProject("Old Project", updatedDTO);
    }

    [TestMethod]
    public void GetAllProjects_ShouldReturnAllProjects()
    {
        ProjectDTO projectDTO1 = new ProjectDTO
        {
            Name = "Project 1",
            Description = "Description 1",
            StartDate = DateTime.Now,
            Members = members
        };
        ProjectDTO projectDTO2 = new ProjectDTO
        {
            Name = "Project 2",
            Description = "Description 2",
            StartDate = DateTime.Now.AddDays(1),
            Members = members
        };

        _adminPservice.CreateProject(projectDTO1);
        _adminPservice.CreateProject(projectDTO2);

        List<ProjectDTO> projects = _adminPservice.GetAllProjects();

        Assert.AreEqual(2, projects.Count);
        Assert.AreEqual("Project 1", projects[0].Name);
        Assert.AreEqual("Project 2", projects[1].Name);
    }

    [TestMethod]
    public void GetProjectByName_ShouldReturnProject_WhenProjectExists()
    {
        ProjectDTO projectDTO = new ProjectDTO
        {
            Name = "Test Project",
            Description = "Test Description",
            StartDate = DateTime.Now,
            AdminProyect = UserDTO,
            Members = members
        };

        _adminPservice.CreateProject(projectDTO);

        ProjectDTO project = _adminPservice.GetProjectByName("Test Project");

        Assert.IsNotNull(project);
        Assert.AreEqual("Test Project", project.Name);
        Assert.AreEqual("Test Description", project.Description);
    }

    [TestMethod]
    public void GetAllProjectsForUsers_ShouldReturnListOfProjects_WhenUserIsAdminOrMember()
    {
        ProjectDTO projectDTO = new ProjectDTO
        {
            Name = "Test Project",
            Description = "Test Description",
            StartDate = DateTime.Now,
            AdminProyect = UserDTO,
            Members = members
        };

        _adminPservice.CreateProject(projectDTO);

        List<ProjectDTO> project = _adminPservice.GetAllProjectsForUser(UserDTO.Email);

        Assert.IsNotNull(project);
        Assert.AreEqual("Test Project", project[0].Name);
        Assert.AreEqual("Test Description", project[0].Description);
    }

    [TestMethod]
    public void GetMembers_ShouldReturnListOfMembers_WhenProjectExist()
    {
        ProjectDTO projectDTO = new ProjectDTO
        {
            Name = "Test Project",
            Description = "Test Description",
            StartDate = DateTime.Now,
            AdminProyect = UserDTO,
            Members = members
        };
        _adminPservice.CreateProject(projectDTO);
        List<UserDTO> projectMembers = _adminPservice.GetMembers("Test Project");
        Assert.IsNotNull(projectMembers);
        Assert.AreEqual(1, projectMembers.Count);
        Assert.AreEqual("User", projectMembers[0].FirstName);
    }

    [TestMethod]
    public void AddTaskToMember_WhenUserIsMember_ShouldAddTaskToMember()
    {
        ProjectDTO projectDTO = new ProjectDTO
        {
            Name = "Test Project",
            Description = "Test Description",
            StartDate = DateTime.Today,
            AdminProyect = UserDTO,
            Members = members
        };
        _adminPservice.CreateProject(projectDTO);
        TaskDTO task = new TaskDTO
        {
            Title = "Task1",
            Description = "Description",
            Duration = 1,
            ExpectedStartDate = DateTime.Today
        };

        _taskService.AddTask("Test Project", task);

        _adminPservice.AddTaskToMember("Test Project", "member.user@example.com", task.Title);

        Assert.IsTrue(_userservice.GetUser("member.user@example.com").Tasks.Any(t => t.Id == 1));
    }

    [TestMethod]
    [ExpectedException(typeof(UserIsNotAMemberException))]
    public void AddTaskToMember_WhenUserIsNotMember_ShouldThrowUserIsNotAMemberException()
    {
        ProjectDTO projectDTO = new ProjectDTO
        {
            Name = "Test Project",
            Description = "Test Description",
            StartDate = DateTime.Today,
            AdminProyect = UserDTO,
            Members = members
        };
        _adminPservice.CreateProject(projectDTO);
        TaskDTO task = new TaskDTO
        {
            Title = "Task1",
            Description = "Description",
            Duration = 1,
            ExpectedStartDate = DateTime.Today
        };
        _taskService.AddTask("Test Project", task);

        List<TaskDTO> addedTasks = _taskService.GetTasks("Test Project");
        TaskDTO addedTask = addedTasks.FirstOrDefault(t => t.Title == "Task1");

        _adminPservice.AddTaskToMember("Test Project", "member1.user@example.com", addedTask.Title);
    }

    [TestMethod]
    [ExpectedException(typeof(TaskIsNotFromTheProjectException))]
    public void AddTaskToMember_WhenTaskIsNotFromTheProject_ShouldThrowException()
    {
        ProjectDTO projectDTO = new ProjectDTO
        {
            Name = "Test Project",
            Description = "Test Description",
            StartDate = DateTime.Today,
            AdminProyect = UserDTO,
            Members = members
        };

        _adminPservice.CreateProject(projectDTO);

        TaskDTO task = new TaskDTO
        {
            Title = "Task1",
            Description = "Description",
            Duration = 1,
            ExpectedStartDate = DateTime.Today
        };

        _taskService.AddTask("Test Project", task);

        _adminPservice.AddTaskToMember("Test Project", "member.user@example.com", "task2");
    }

    [TestMethod]
    public void RemoveTaskFromMember_WhenUserIsMember_ShouldRemoveTaskFromMember()
    {
        ProjectDTO projectDTO = new ProjectDTO
        {
            Name = "Test Project",
            Description = "Test Description",
            StartDate = DateTime.Today,
            AdminProyect = UserDTO,
            Members = members
        };
        _adminPservice.CreateProject(projectDTO);
        TaskDTO task = new TaskDTO
        {
            Title = "Task1",
            Description = "Description",
            Duration = 1,
            ExpectedStartDate = DateTime.Today
        };

        _taskService.AddTask("Test Project", task);

        _adminPservice.AddTaskToMember("Test Project", "member.user@example.com", task.Title);

        Assert.IsTrue(_userservice.GetUser("member.user@example.com").Tasks.Any(t => t.Id == 1));

        _adminPservice.RemoveTaskFromMember("Test Project", "member.user@example.com", task.Title);

        Assert.IsTrue(_userservice.GetUser("member.user@example.com").Tasks.Count == 0);
    }

    [TestMethod]
    [ExpectedException(typeof(UserIsNotAMemberException))]
    public void RemoveTaskFromMember_WhenUserIsNotMember_ShouldThrowUserIsNotAMemberException()
    {
        ProjectDTO projectDTO = new ProjectDTO
        {
            Name = "Test Project",
            Description = "Test Description",
            StartDate = DateTime.Today,
            AdminProyect = UserDTO,
            Members = members
        };
        _adminPservice.CreateProject(projectDTO);
        TaskDTO task = new TaskDTO
        {
            Title = "Task1",
            Description = "Description",
            Duration = 1,
            ExpectedStartDate = DateTime.Today
        };

        _taskService.AddTask("Test Project", task);

        _adminPservice.AddTaskToMember("Test Project", "member.user@example.com", task.Title);
        _adminPservice.RemoveTaskFromMember("Test Project", "member1.user@example.com", task.Title);
    }

    [TestMethod]
    [ExpectedException(typeof(TaskIsNotFromTheProjectException))]
    public void RemoveTaskFromMember_WhenTaskIsNotFromTheProject_ShouldThrowException()
    {
        ProjectDTO projectDTO = new ProjectDTO
        {
            Name = "Test Project",
            Description = "Test Description",
            StartDate = DateTime.Today,
            AdminProyect = UserDTO,
            Members = members
        };
        _adminPservice.CreateProject(projectDTO);
        TaskDTO task = new TaskDTO
        {
            Title = "Task1",
            Description = "Description",
            Duration = 1,
            ExpectedStartDate = DateTime.Today
        };


        _taskService.AddTask("Test Project", task);

        _adminPservice.AddTaskToMember("Test Project", "member.user@example.com", task.Title);
        _adminPservice.RemoveTaskFromMember("Test Project", "member.user@example.com", "task2");
    }

    [TestMethod]
    public void GetTasksForAMember_WhenGettingTasksForAMember_ShouldReturnListOfTasks()
    {
        ProjectDTO projectDTO = new ProjectDTO
        {
            Name = "Test Project",
            Description = "Test Description",
            StartDate = DateTime.Today,
            AdminProyect = UserDTO,
            Members = members
        };
        _adminPservice.CreateProject(projectDTO);
        TaskDTO task = new TaskDTO
        {
            Title = "Task1",
            Description = "Description",
            Duration = 1,
            ExpectedStartDate = DateTime.Today
        };
        TaskDTO task2 = new TaskDTO
        {
            Title = "Task2",
            Description = "Description2",
            Duration = 1,
            ExpectedStartDate = DateTime.Today
        };


        _taskService.AddTask("Test Project", task);
        _taskService.AddTask("Test Project", task2);

        _adminPservice.AddTaskToMember("Test Project", "member.user@example.com", task.Title);
        _adminPservice.AddTaskToMember("Test Project", "member.user@example.com", task2.Title);

        List<TaskDTO> tasks = _adminPservice.GetAllTaskForAMember("member.user@example.com");

        Assert.AreEqual(2, tasks.Count);
    }

    [TestMethod]
    public void GetTasksForAMemberInAProject_WhenGettingTasksForAMember_ShouldReturnListOfTasks()
    {
        ProjectDTO projectDTO = new ProjectDTO
        {
            Name = "Test Project",
            Description = "Test Description",
            StartDate = DateTime.Today,
            AdminProyect = UserDTO,
            Members = members
        };
        _adminPservice.CreateProject(projectDTO);
        TaskDTO task = new TaskDTO
        {
            Title = "Task1",
            Description = "Description",
            Duration = 1,
            ExpectedStartDate = DateTime.Today
        };
        TaskDTO task2 = new TaskDTO
        {
            Title = "Task2",
            Description = "Description2",
            Duration = 1,
            ExpectedStartDate = DateTime.Today
        };

        _taskService.AddTask("Test Project", task);
        _taskService.AddTask("Test Project", task2);

        _adminPservice.AddTaskToMember("Test Project", "member.user@example.com", task.Title);
        _adminPservice.AddTaskToMember("Test Project", "member.user@example.com", task2.Title);

        List<TaskDTO> tasks = _adminPservice.GetAllTaskForAMemberInAProject("Test Project", "member.user@example.com");

        Assert.AreEqual(2, tasks.Count);
    }

    [TestMethod]
    public void SetProjectLeader_WhenTheUserIsNotAlredyProjectLeader_ShouldBeOkey()
    {
        ProjectDTO projectDTO = new ProjectDTO
        {
            Name = "Test Project",
            Description = "Test Description",
            StartDate = DateTime.Today,
            AdminProyect = UserDTO,
            Members = members
        };
        _adminPservice.CreateProject(projectDTO);

        _adminPservice.SetProjectLeader(projectDTO.Name, Leader.Email);
    }

    [TestMethod]
    public void SetProjectLeader_ShouldAssignLeader_WhenValid()
    {
        ProjectDTO projectDTO = new ProjectDTO
        {
            Name = "New Project",
            Description = "Description",
            StartDate = DateTime.Now,
            AdminProyect = UserDTO,
            Members = members
        };

        _adminPservice.CreateProject(projectDTO);

        _adminPservice.SetProjectLeader("New Project", Leader.Email);

        Project project = _repositoryManager.ProjectRepository.Get(p => p.Name == "New Project");
        Assert.AreEqual(Leader.Email, project.ProjectLeader.Email);
    }


    [TestMethod]
    public void GetAllProjectLeaderUsers_ShouldReturnOnlyProjectLeaders()
    {
        List<UserDTO> projectLeaders = _adminPservice.GetAllProjectLeaderUsers();

        Assert.AreEqual(1, projectLeaders.Count, "There should be exactly one project leader");
        Assert.AreEqual(Leader.FirstName, projectLeaders[0].FirstName,
            "The first leader should be the one with the correct name");
        Assert.AreEqual(Leader.Email, projectLeaders[0].Email, "The leader should have the correct email");
    }
    
    [TestMethod]
    public void GetTasks_ShouldReturnAllTasks_WhenProjectHasTasks()
    {
        ProjectDTO projectDTO = new ProjectDTO
        {
            Name = "Test Project",
            Description = "Test Description",
            StartDate = DateTime.Today,
            AdminProyect = UserDTO,
            Members = members
        };
        _adminPservice.CreateProject(projectDTO);

        TaskDTO task1 = new TaskDTO
        {
            Title = "Task 1",
            Description = "Task 1 Description",
            Duration = 2,
            ExpectedStartDate = DateTime.Today
        };
        TaskDTO task2 = new TaskDTO
        {
            Title = "Task 2",
            Description = "Task 2 Description",
            Duration = 3,
            ExpectedStartDate = DateTime.Today.AddDays(1)
        };

        _taskService.AddTask("Test Project", task1);
        _taskService.AddTask("Test Project", task2);

        List<TaskDTO> tasks = _adminPservice.GetTasks(projectDTO);

        Assert.AreEqual(2, tasks.Count);
        Assert.AreEqual("Task 1", tasks[0].Title);
        Assert.AreEqual("Task 2", tasks[1].Title);
    }
    
    [TestMethod]
    [ExpectedException(typeof(ProjectNotFoundException))]
    public void GetTasks_ShouldThrowException_WhenProjectDoesNotExist()
    {
        ProjectDTO projectDTO = new ProjectDTO
        {
            Name = "Nonexistent Project",
            Description = "Nonexistent Project Description",
            StartDate = DateTime.Today
        };

        _adminPservice.GetTasks(projectDTO);
    }
}