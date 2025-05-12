using Domain;
using Service.Models;

namespace Service.Interfaces;

public interface IMemberPService
{
    List<ProjectDTO> GetAllProjectsFromAMember(string email);
    void ChangeTaskStatus(string projectName, string email, TaskDTO task, State status);
}