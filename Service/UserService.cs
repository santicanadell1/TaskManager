using DataAccess;
using DataAccess.Exceptions.UserRepositoryExceptions;
using Domain;
using Service.Exceptions.UserServiceExceptions;
using Service.Interface;
using Service.Models;
using Task = Domain.Task;

namespace Service;

public class UserService : IUserService
{
    private readonly UserRepository _userRepository;
    private readonly PasswordManager _passwordManager = new();

    public UserService(UserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public void AddUser(UserDTO userDTO)
    {
        if (_userRepository.GetAll().Count == 0) userDTO.Roles.Add(RolDTO.AdminSystem);
        ValidateUserEmailAndPassword(userDTO);
        _userRepository.Add(ToEntity(userDTO));
    }

    public void UpdateUser(UserDTO userDTO)
    {
        User user;
        if (userDTO.Id.HasValue)
        {
            user = _userRepository.Get(u => u.Id == userDTO.Id);
            if (user == null) throw new UserNotFoundException();
        }
        else
        {
            user = GetUserObject(userDTO.Email);
        }

        user.FirstName = userDTO.FirstName;
        user.LastName = userDTO.LastName;
        user.Email = userDTO.Email;
        user.Roles = ConvertToDomainRoles(userDTO.Roles);
        user.Birthday = userDTO.Birthday;
        user.Password = userDTO.Password;
        user.Tasks = getTasksEntity(userDTO.Tasks);
        _userRepository.Update(user);
    }

    public List<UserDTO> GetUsers()
    {
        List<UserDTO> usersDTO = new List<UserDTO>();

        foreach (var user in _userRepository.GetAll()) usersDTO.Add(FromEntity(user));

        if (usersDTO.Count == 0) throw new NoUsersFoundException();

        return usersDTO;
    }

    public UserDTO GetUser(string email)
    {
        var user = _userRepository.Get(user => user.Email == email);
        if (user == null) throw new UserNotFoundException();

        return FromEntity(user);
    }


    private void ValidateUserEmailAndPassword(UserDTO userDTO)
    {
        foreach (var user in _userRepository.GetAll())
            if (user.Email == userDTO.Email)
                throw new InvalidUserEmailException();

        if (!_passwordManager.IsValidPassword(userDTO.Password)) throw new InvalidUserPasswordException();
    }

    private User ToEntity(UserDTO userDTO)
    {
        return new User
        {
            Id = userDTO.Id,
            Email = userDTO.Email,
            FirstName = userDTO.FirstName,
            LastName = userDTO.LastName,
            Password = _passwordManager.HashPassword(userDTO.Password),
            Birthday = userDTO.Birthday,
            Roles = ConvertToDomainRoles(userDTO.Roles),
            Tasks = getTasksEntity(userDTO.Tasks)
        };
    }

    private List<Task> getTasksEntity(List<TaskDTO> tasks)
    {
        List<Task> ret = new List<Task>();
        if (tasks == null) return ret;

        foreach (var task in tasks)
        {
            ret.Add(ToEntityTask(task));
        }

        return ret;
    }

    private Task ToEntityTask(TaskDTO taskDTO)
    {
        return new Task(
            taskDTO.Title,
            taskDTO.Description,
            taskDTO.ExpectedStartDate,
            taskDTO.Duration,
            ToEntityList(taskDTO.PreviousTasks),
            ToEntityList(taskDTO.SameTimeTasks),
            ToResourceEntityList(taskDTO.Resources)
        );
    }

    private List<Task> ToEntityList(List<TaskDTO> taskDTOs)
    {
        if (taskDTOs == null) return new List<Task>();

        var tasks = new List<Task>();
        foreach (var taskDTO in taskDTOs)
        {
            tasks.Add(ToEntityTask(taskDTO));
        }

        return tasks;
    }

    private List<Resource> ToResourceEntityList(List<ResourceDTO> resourceDTOs)
    {
        if (resourceDTOs == null) return new List<Resource>();

        var resources = new List<Resource>();
        foreach (var resourceDTO in resourceDTOs)
            resources.Add(new Resource(resourceDTO.Name, resourceDTO.Type, resourceDTO.Description)
                { Id = resourceDTO.Id });

        return resources;
    }

    private User GetUserObject(string email)
    {
        var user = _userRepository.Get(user => user.Email == email);
        if (user == null) throw new UserNotFoundException();

        return user;
    }

    private UserDTO FromEntity(User user)
    {
        return new UserDTO
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Roles = ConvertToDTORoles(user.Roles),
            Password = user.Password,
            Birthday = user.Birthday,
            Tasks = FromEntityList(user.Tasks)
        };
    }

    private List<Rol> ConvertToDomainRoles(List<RolDTO> roleDTOs)
    {
        var roles = new List<Rol>();

        foreach (var roleDTO in roleDTOs)
            switch (roleDTO)
            {
                case RolDTO.AdminSystem:
                    roles.Add(Rol.AdminSystem);
                    break;
                case RolDTO.ProjectMember:
                    roles.Add(Rol.ProjectMember);
                    break;
                case RolDTO.AdminProject:
                    roles.Add(Rol.AdminProject);
                    break;
            }

        return roles;
    }

    private List<RolDTO> ConvertToDTORoles(List<Rol> roles)
    {
        var roleDTOs = new List<RolDTO>();

        foreach (var role in roles)
            switch (role)
            {
                case Rol.AdminSystem:
                    roleDTOs.Add(RolDTO.AdminSystem);
                    break;
                case Rol.ProjectMember:
                    roleDTOs.Add(RolDTO.ProjectMember);
                    break;
                case Rol.AdminProject:
                    roleDTOs.Add(RolDTO.AdminProject);
                    break;
            }

        return roleDTOs;
    }

    private List<TaskDTO> FromEntityList(List<Task> tasks)
    {
        List<TaskDTO> taskDTOs = new List<TaskDTO>();
        foreach (var task in tasks) taskDTOs.Add(FromEntityTask(task));
        return taskDTOs;
    }

    private TaskDTO FromEntityTask(Task task)
    {
        return new TaskDTO
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            ExpectedStartDate = task.ExpectedStartDate,
            Duration = task.Duration,
            PreviousTasks = ToTaskDTOList(task.PreviousTasks),
            SameTimeTasks = ToTaskDTOList(task.SameTimeTasks),
            State = (StateDTO)task.State,
            Resources = FromResourceEntityList(task.Resources) ?? new List<ResourceDTO>(),
            IsCritical = task.IsCritical,
            StartDate = task.StartDate,
            EndDate = task.EndDate,
            LatestStart = task.LatestStart,
            LatestFinish = task.LatestFinish,
            Slack = task.Slack
        };
    }

    private List<TaskDTO> ToTaskDTOList(List<Task> tasks)
    {
        if (tasks == null) return new List<TaskDTO>();

        return tasks.Select(task => new TaskDTO
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            ExpectedStartDate = task.ExpectedStartDate,
            Duration = task.Duration,
            PreviousTasks = ToTaskDTOList(task.PreviousTasks),
            SameTimeTasks = ToTaskDTOList(task.SameTimeTasks),
            State = (StateDTO)task.State,
            Resources = FromResourceEntityList(task.Resources) ?? new List<ResourceDTO>(),
            IsCritical = task.IsCritical,
            StartDate = task.StartDate,
            EndDate = task.EndDate,
            LatestStart = task.LatestStart,
            LatestFinish = task.LatestFinish,
            Slack = task.Slack
        }).ToList();
    }

    private List<ResourceDTO> FromResourceEntityList(List<Resource> resources)
    {
        var resourceDTOs = new List<ResourceDTO>();
        foreach (var resource in resources)
            resourceDTOs.Add(new ResourceDTO
            {
                Name = resource.Name,
                Type = resource.Type,
                Description = resource.Description,
                Id = resource.Id
            });

        return resourceDTOs;
    }
}