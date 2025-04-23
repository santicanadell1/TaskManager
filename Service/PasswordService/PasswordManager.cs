using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Text;

namespace Service.Passsword;

public static class PasswordManager
{
    public static string getDefaultPassword()
    {
        string defaultPassword = "Password123?";
        return defaultPassword;
    }

    public static bool IsValidPassword(string password)
    {
        string passwordPattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_])[A-Za-z\d\W_]{8,}$";

        return Regex.IsMatch(password, passwordPattern);
    }

   
    // Hashing function using SHA-256 algorithm to hash the password
    public static string HashPassword(string password)
    {
        // Create an SHA256 instance
        using (SHA256 sha256Hash = SHA256.Create())
        {
            // Convert the password string into a byte array
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));

            // Convert the byte array to a hexadecimal string
            StringBuilder builder = new StringBuilder();
            foreach (byte b in bytes)
            {
                builder.Append(b.ToString("x2"));  // Convert each byte to a two-character hexadecimal string
            }
                
            // Return the resulting hash as a string
            return builder.ToString();
        }
    }

}