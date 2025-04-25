using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Text;
using Service.Interfaces;

public class PasswordManager : IPasswordManager
{
    // Returns the default password
    public string getDefaultPassword()
    {
        var defaultPassword = "Password123?"; // Set a default password
        return defaultPassword;
    }

    // Validates if the provided password matches the required pattern
    public bool IsValidPassword(string password)
    {
        var passwordPattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_])[A-Za-z\d\W_]{8,}$";
        return Regex.IsMatch(password, passwordPattern); // Matches the password with the pattern
    }

   

    // Hashing function using SHA-256 algorithm to hash the password
    public string HashPassword(string password)
    {
        // Create an SHA256 instance
        using (var sha256Hash = SHA256.Create())
        {
            // Convert the password string into a byte array
            var bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));

            // Convert the byte array to a hexadecimal string
            var builder = new StringBuilder();
            foreach (var b in
                     bytes) builder.Append(b.ToString("x2")); // Convert each byte to a two-character hexadecimal string

            // Return the resulting hash as a string
            return builder.ToString();
        }
    }

    // Verifies if the provided plain password matches the stored hash
    public bool VerifyPassword(string plainPassword, string storedHash)
    {
        var hashedPassword = HashPassword(plainPassword); // Hash the plain password
        return hashedPassword == storedHash; // Compare with the stored hash
    }
}