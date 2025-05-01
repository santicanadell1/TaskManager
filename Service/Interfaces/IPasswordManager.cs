namespace Service.Interfaces
{
    public interface IPasswordManager
    {
        // Method to get the default password
        string getDefaultPassword();

        // Method to validate a password based on a predefined pattern
        bool IsValidPassword(string password);

        // Method to hash a password using SHA-256
        string HashPassword(string password);

        // Method to verify if a plain password matches the stored hash
        bool VerifyPassword(string plainPassword, string storedHash);
    }
}