using Service.Models;

namespace Service.Interface;

public interface IUserService
{
    void AddUser(UserDTO userDTO);
    void UpdateUser(UserDTO userDTO);
    List<UserDTO> GetUsers();
    UserDTO GetUser(string email);
}