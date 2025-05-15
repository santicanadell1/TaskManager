namespace Service.Interface;

public interface IPasswordManager
{
    string getDefaultPassword();

    bool IsValidPassword(string password);

    string HashPassword(string password);

    bool VerifyPassword(string plainPassword, string storedHash);
}