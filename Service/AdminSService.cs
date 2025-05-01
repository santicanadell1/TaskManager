using DataAccess;
using Domain;
using Domain.Exceptions;
using Service;
using Service.Models;

public class AdminSService
{
    private readonly InMemoryDatabase _database;
    private UserService _userService;

    public AdminSService(InMemoryDatabase database)
    {
        _database = database;
        _userService = new UserService(_database);
    }

    private void CheckAdminRole()
    {
        var currentUser = LoggedUser.Current;
        if (currentUser == null || !currentUser.Roles.Contains(Rol.AdminSystem))
        {
            throw new UnauthorizedAdminAccessException();
        }
    }

    public void CreateUser(UserDTO userDTO)
    {
        CheckAdminRole();
        _userService.AddUser(userDTO);
    }
    
    public void DeleteUser(UserDTO userDTO)
    {
        CheckAdminRole(); 

        var user = _userService.GetUser(userDTO.Email);  

        if (user == null)
        {
            throw new UserNotFoundException(); 
        }

        _database.users.Delete(user.Email);  
    }
    
    public void ChangePassword(string email, string newPassword)
    {
        CheckAdminRole();  

        var user = _userService.GetUser(email);  

        if (user == null)
        {
            throw new UserNotFoundException();  
        }

        user.Password = newPassword;  
        _userService.UpdateUser(user);  
    }



}