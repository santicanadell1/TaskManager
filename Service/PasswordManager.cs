using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Service.Interface;

public class PasswordManager : IPasswordManager
{
    private readonly Random _random = new();

    public string getDefaultPassword()
    {
        return GenerateRandomPassword();
    }

    public bool IsValidPassword(string password)
    {
        String passwordPattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_])[A-Za-z\d\W_]{8,}$";
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

    private string GenerateRandomPassword()
    {
        const string lowerChars = "abcdefghijklmnopqrstuvwxyz";
        const string upperChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string digits = "1234567890";
        const string specialChars = "!@#$%^&*()_+[]{}|;:,.<>?";

        StringBuilder password = new StringBuilder();
        password.Append(lowerChars[_random.Next(lowerChars.Length)]);
        password.Append(upperChars[_random.Next(upperChars.Length)]);
        password.Append(digits[_random.Next(digits.Length)]);
        password.Append(specialChars[_random.Next(specialChars.Length)]);

        const string allValidChars =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()_+[]{}|;:,.<>?";
        while (password.Length < 16) password.Append(allValidChars[_random.Next(allValidChars.Length)]);

        return new string(password.ToString().OrderBy(c => _random.Next()).ToArray());
    }
}