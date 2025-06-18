using System.Text.RegularExpressions;
using Domain.Exceptions.UserExceptions;

namespace Domain;

public class User
{
    public DateTime birthday;
    public string email;
    public string firstName;
    public string lastName;
    public string password;
    public List<Rol> roles = new();
    public List<Task> tasks = new();


    public User(string firstName, string lastName, string email, DateTime birthday, string password)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Birthday = birthday;
        Password = password;
        Roles = new List<Rol>();
        Tasks = new List<Task>();
    }

    public User()
    {
    }

    public int? Id { get; set; }


    public string FirstName
    {
        get => firstName;
        set =>
            firstName = string.IsNullOrWhiteSpace(value) ? throw new UserFirstNameException() : value;
    }

    public string LastName
    {
        get => lastName;
        set =>
            lastName = string.IsNullOrWhiteSpace(value) ? throw new UserLastNameException() : value;
    }

    public string Email
    {
        get => email;
        set
        {
            if (string.IsNullOrWhiteSpace(value)) throw new UserEmailException();

            email = IsValidEmail(value) ? value : throw new UserEmailException();
        }
    }

    public DateTime Birthday
    {
        get => birthday;
        set
        {
            if (DateTime.Today.AddYears(-18) < value || value < DateTime.Today.AddYears(-100))
                throw new UserBirthdayException();
            birthday = value;
        }
    }

    public string Password
    {
        get => password;
        set =>
            password = string.IsNullOrWhiteSpace(value) ? throw new UserPasswordException() : value;
    }

    public List<Notification> Notifications { get; set; } = new();

    public List<Rol> Roles
    {
        get => roles;
        set
        {
            if (value == null) throw new UserRolesInvalidAssignmentException();

            roles = value;
        }
    }

    public List<Task> Tasks
    {
        get => tasks;
        set
        {
            if (value == null) tasks = new List<Task>();
            tasks = value;
        }
    }

    private bool IsValidEmail(string email)
    {
        var emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+.[a-zA-Z]{2,}$";
        return Regex.IsMatch(email, emailPattern);
    }

    public void AddRol(Rol rol)
    {
        if (roles.Contains(rol)) throw new UserRoleAlreadyExistsException(rol.ToString());

        roles.Add(rol);
    }

    public void RemoveRol(Rol rol)
    {
        if (!roles.Contains(rol)) throw new UserRoleNotFoundException(rol.ToString());

        roles.Remove(rol);
    }

    public void AddTask(Task task)
    {
        if (tasks == null) Tasks = new List<Task>();
        if (tasks.Any(t => t.Id == task.Id)) throw new UserTaskException("The task is already assigned to the user.");
        tasks.Add(task);
    }

    public void RemoveTask(Task task)
    {
        if (!tasks.Any(t => t.Id == task.Id)) throw new UserTaskException("the task is not assigned to the user.");
        tasks.Remove(task);
    }

    public void AddNotification(Notification notification)
    {
        Notifications.Add(notification);
    }

    public void RemoveNotification(Notification notification)
    {
        Notifications.Remove(notification);
    }
}