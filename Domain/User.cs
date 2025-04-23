using System.Text.RegularExpressions;

namespace Domain;

public class User
{
    private string firstName;
    private string lastName;
    private string email;
    private DateTime birthday;
    private string password;
    
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

    public DateTime Birthday { get => birthday; set => birthday = value; }
    private string Password { get => password; set => password = value; }

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
}