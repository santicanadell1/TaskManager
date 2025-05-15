using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Service.Interface;

public class PasswordManager : IPasswordManager
{
    public string getDefaultPassword()
    {
        var defaultPassword = "Password123?";
        return defaultPassword;
    }

    public bool IsValidPassword(string password)
    {
        var passwordPattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_])[A-Za-z\d\W_]{8,}$";
        return Regex.IsMatch(password, passwordPattern);
    }

    public string HashPassword(string password)
    {
        using (var sha256Hash = SHA256.Create())
        {
            var bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
            var builder = new StringBuilder();
            foreach (var b in
                     bytes) builder.Append(b.ToString("x2"));

            return builder.ToString();
        }
    }

    public bool VerifyPassword(string plainPassword, string storedHash)
    {
        var hashedPassword = HashPassword(plainPassword);
        return hashedPassword == storedHash;
    }
}