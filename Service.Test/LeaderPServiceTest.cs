using DataAccess;
using DataAccess.Exceptions.ProjectRepositoryExceptions;
using DataAccess.Exceptions.TaskRepositoryExceptions;
using Domain;
using Service.Exceptions.AdminPServiceExceptions;
using Service.Exceptions.AdminSServiceExceptions;
using Service.Exceptions.LeaderPServiceException;
using Service.Models;
using Task = Domain.Task;

namespace Service.Test;

[TestClass]
public class LeaderPService_Test
{
    private LeaderPService _leaderService;
    private AdminPService _adminService;
    private TaskService _taskService;
    private AppDbContext _context;
    private Login _loginService;
    private UserService _userService;
    private IRepositoryManager _repositoryManager;
    private CpmService _cpmService;

    [TestInitialize]
    public void TestSetUp()
    {
        InMemoryAppContextFactory contextFactory = new InMemoryAppContextFactory();
        _context = contextFactory.CreateDbContext();

        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();

        _repositoryManager = new RepositoryManager(_context);
        _cpmService = new CpmService();

        _leaderService = new LeaderPService(_repositoryManager);
        _adminService = new AdminPService(_repositoryManager);
        _taskService = new TaskService(_repositoryManager, _cpmService);
        _loginService = new Login(_repositoryManager);
        _userService = new UserService(_repositoryManager);

        UserDTO adminUserDTO = new UserDTO
        {
            FirstName = "Admin",
            LastName = "User",
            Email = "admin.user@example.com",
            Password = "AdminPassword123@",
            Birthday = DateTime.Parse("1990-01-01"),
            Roles = new List<RolDTO> { RolDTO.AdminProject }
        };

        UserDTO leaderUserDTO = new UserDTO
        {
            FirstName = "Leader",
            LastName = "User",
            Email = "leader.user@example.com",
            Password = "LeaderPassword123@",
            Birthday = DateTime.Parse("1990-01-01"),
            Roles = new List<RolDTO> { RolDTO.ProjectLeader }
        };

        UserDTO normalUserDTO = new UserDTO
        {
            FirstName = "Normal",
            LastName = "User",
            Email = "normal.user@example.com",
            Password = "Password123@",
            Birthday = DateTime.Parse("1990-01-01"),
            Roles = new List<RolDTO> { RolDTO.ProjectMember }
        };

        _userService.AddUser(adminUserDTO);
        _userService.AddUser(leaderUserDTO);
        _userService.AddUser(normalUserDTO);

        _loginService.LoginUser("admin.user@example.com", "AdminPassword123@");

        ProjectDTO testProject = new ProjectDTO
        {
            Name = "Test Project",
            Description = "Test project description",
            StartDate = DateTime.Now.AddDays(1),
            AdminProyect = adminUserDTO,
            ProjectLeader = leaderUserDTO,
            Members = new List<UserDTO>()
        };

        _adminService.CreateProject(testProject);
    }

    [TestCleanup]
    public void CleanUp()
    {
        _context?.Database.EnsureDeleted();
    }
    
    [TestMethod]
    [ExpectedException(typeof(UnauthorizedLeaderAccessException))]
    public void LeaderPService_ShouldThrowUnauthorizedAccessException_WhenUserIsNotProjectLeader()
    {
        _loginService.LoginUser("normal.user@example.com", "Password123@");

        TaskDTO taskDTO = new TaskDTO
        {
            Title = "Test Task",
            Description = "Test task description",
            ExpectedStartDate = DateTime.Now.AddDays(2),
            Duration = 5,
            State = StateDTO.TODO,
            Resources = new List<ResourceDTO>()
        };

        _leaderService.AddTask("Test Project", taskDTO);
    }

    
}