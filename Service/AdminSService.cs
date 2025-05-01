using DataAccess;
using Domain;
using Domain.Exceptions;
using Service;
using Service.Models;

public class AdminSService
{
    private readonly InMemoryDatabase _database;

    public AdminSService(InMemoryDatabase database)
    {
        _database = database;
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
        var userService = new UserService(_database);
        userService.AddUser(userDTO);
    }
    
    public void DeleteUser(UserDTO userDTO)
    {
        CheckAdminRole(); 

        var userService = new UserService(_database);
        var user = userService.GetUser(userDTO.Email);  

        if (user == null)
        {
            throw new UserNotFoundException(); 
        }

        _database.users.Delete(user.Email);  
    }


}