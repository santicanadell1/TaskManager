using System.Text.RegularExpressions;

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
}