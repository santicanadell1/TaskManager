using Service.Models;

namespace Service.Interface;

public interface IMemberPService
{
    List<ProjectDTO> GetAllProjectsFromAMember(string email);
    void ChangeTaskStatus(string projectName, string email, TaskDTO task, StateDTO status);
}