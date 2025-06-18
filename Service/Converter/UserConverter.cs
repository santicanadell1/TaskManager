using DataAccess;
using Domain;
using Service.Models;
using Task = Domain.Task;

namespace Service.Converter;

public class UserConverter : IConverter<User, UserDTO>
{
    private readonly PasswordManager _passwordManager = new();
    private readonly IRepositoryManager _repositoryManager;
    private readonly RolConverter _rolConverter;
    private readonly TaskConverter _taskConverter;

    public UserConverter(IRepositoryManager repositoryManager)
    {
        _repositoryManager = repositoryManager;
        _rolConverter = new RolConverter();
        _taskConverter = new TaskConverter(repositoryManager);
    }

    public UserDTO FromEntity(User user)
    {
        return new UserDTO
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Roles = _rolConverter.ConvertToDTORoles(user.Roles),
            Password = user.Password,
            Birthday = user.Birthday,
            Tasks = FromEntityList(user.Tasks)
        };
    }

    public User ToEntity(UserDTO userDTO)
    {
        return new User
        {
            Id = userDTO.Id,
            Email = userDTO.Email,
            FirstName = userDTO.FirstName,
            LastName = userDTO.LastName,
            Password = _passwordManager.HashPassword(userDTO.Password),
            Birthday = userDTO.Birthday,
            Roles = _rolConverter.ConvertToDomainRoles(userDTO.Roles),
            Tasks = getTasksEntity(userDTO.Tasks)
        };
    }

    public List<Task> getTasksEntity(List<TaskDTO> taskDTOs)
    {
        return taskDTOs?.Select(dto => _taskConverter.ToEntity(dto)).ToList() ?? new List<Task>();
    }


    public List<TaskDTO> FromEntityList(List<Task> tasks)
    {
        if (tasks == null) return new List<TaskDTO>();
        return tasks.Select(t => _taskConverter.FromEntity(t)).ToList();
    }
}