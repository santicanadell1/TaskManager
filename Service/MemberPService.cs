using DataAccess;
using Domain;
using Domain.Exceptions;
using Service.Models;

namespace Service;

public class MemberPService
{
    private InMemoryDatabase _database;
    public MemberPService(InMemoryDatabase database)
    {
        _database = database;
    }
    
    public List<ProjectDTO> GetAllProjectsFromAMember(string email)
    {
        AdminPService adminPService = new AdminPService(_database);
        UserService userService = new UserService(_database);
        List<ProjectDTO> projectsFromMember = new List<ProjectDTO>();
        List<ProjectDTO> projects = adminPService.GetAllProjects();
        UserDTO user = userService.GetUser(email);
        if (!user.Roles.Contains(Rol.ProjectMember))
        {
            throw new UserIsNotAMemberException();
        }
        foreach (var project in projects)
        {
            if (project.Members.Any(m => m.Email == user.Email))
            {
                projectsFromMember.Add(project);
            }
        }
        return projectsFromMember;
    }


}