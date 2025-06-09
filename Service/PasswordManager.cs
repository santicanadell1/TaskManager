using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Service.Interface;

public class PasswordManager : IPasswordManager
{
    public string getDefaultPassword()
    {
        string defaultPassword = "Password123?";
        return defaultPassword;
    }

    public bool IsValidPassword(string password)
    {
        string passwordPattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_])[A-Za-z\d\W_]{8,}$";
        return Regex.IsMatch(password, passwordPattern);
    }

    public string HashPassword(string password)
    {
        using (SHA256 sha256Hash = SHA256.Create())
        {
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
            StringBuilder builder = new StringBuilder();
            foreach (byte b in
                     bytes) builder.Append(b.ToString("x2"));

            return builder.ToString();
        }
    }

    public bool VerifyPassword(string plainPassword, string storedHash)
    {
        string hashedPassword = HashPassword(plainPassword);
        return hashedPassword == storedHash;
    }
    
}