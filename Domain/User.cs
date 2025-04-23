namespace Domain;

public class User
{
    private string firstName;
    private string lastName;
    private string email;
    private DateTime birthday;
    private string password;
    
    public string FirstName { get => firstName; set => firstName = value; }
    public string LastName { get => lastName; set => lastName = value; }
    public string Email { get => email; set => email = value; }
    public DateTime Birthday { get => birthday; set => birthday = value; }
    private string Password { get => password; set => password = value; }

    public User(string firstName, string lastName, string email, DateTime birthday, string password)
    {
        this.firstName = firstName; 
        this.lastName = lastName;
        this.email = email;
        this.birthday = birthday;
        this.password = password;
    }
}