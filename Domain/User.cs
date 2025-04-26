using System.Text.RegularExpressions;

namespace Domain;

public class User
{
    private string firstName;
    private string lastName;
    private string email;
    private DateTime birthday;
    private string password;
    private List<Rol> roles;
    public string FirstName
    {
        get => firstName;
        set
        {
            firstName = string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("User name can not be empty") : value;
        }
    }

    public string LastName {
        get => lastName;
        set
        {
            lastName = string.IsNullOrWhiteSpace(value)
                ? throw new ArgumentException("User last name can not be empty")
                : value;
        }
    }

    public string Email
    {
        get => email;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("User email can not be empty");
            }
            email = IsValidEmail(value) ? value : throw new ArgumentException("Invalid email format");
        }
    }

    public DateTime Birthday
    {
        get => birthday;
        set
        {
            birthday = DateTime.Today < value ? throw new ArgumentException("The date is invalid"): value;
        }
    }

    public string Password
    {
        get => password;
        set
        {
            password = string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Password can not be empty") : value;
        }
    }
    
    public List<Rol> Roles
    {
        get => roles;
        set => roles = value ?? throw new ArgumentNullException(nameof(value), "Roles cannot be null");
    }

    public User(string firstName, string lastName, string email, DateTime birthday, string password)
    {
        this.FirstName = firstName; 
        this.LastName = lastName;
        this.Email = email;
        this.Birthday = birthday;
        this.Password = password;
    }

    private bool IsValidEmail(string email)
    {
        string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+.[a-zA-Z]{2,}$";
        return Regex.IsMatch(email, emailPattern);
    }
    
    public void AddRol(Rol rol)
    {
        if (roles.Contains(rol))
        {
            throw new InvalidOperationException($"Role '{rol}' already exists for this user.");
        }

        roles.Add(rol);
    }

    public void RemoveRol(Rol rol)
    {
        if (!roles.Contains(rol))
        {
            throw new InvalidOperationException($"Role '{rol}' does not exist for this user.");
        }

        roles.Remove(rol);
    }
}