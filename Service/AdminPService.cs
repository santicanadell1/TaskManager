using System.Runtime.CompilerServices;
using DataAccess;
using DataAccess.Exceptions.ProjectRepositoryExceptions;
using DataAccess.Exceptions.UserRepositoryExceptions;
using Domain;
using Service;
using Service.Converter;
using Service.Converters;
using Service.Exceptions.AdminPServiceExceptions;
using Service.Exceptions.AdminSServiceExceptions;
using Service.Interface;
using Service.Models;
using Task = Domain.Task;

public class AdminPService : IAdminPService
{
    private readonly IRepositoryManager _repositoryManager;
    private readonly ProjectConverter _projectConverter;
    private readonly UserConverter _userConverter;

    public AdminPService(IRepositoryManager repositoryManager)
    {
        _repositoryManager = repositoryManager;
        _projectConverter = new ProjectConverter(_repositoryManager);
        _userConverter = new UserConverter(_repositoryManager);
    }

    public void CreateProject(ProjectDTO projectDTO)
    {
        CheckAdminProyectRole();
        Project existingProject = _repositoryManager.ProjectRepository.Get(p => p.Name == projectDTO.Name);
        if (existingProject != null) throw new DuplicatedProjectsNameException();

        Project newProject = _projectConverter.ToEntity(projectDTO);
        SetProjectAdmin(newProject, projectDTO);

        _repositoryManager.ProjectRepository.Add(newProject);
    }


    public void AssignMembersToProject(string projectName, List<UserDTO> membersDTO)
    {
        try
        {
            Console.WriteLine($"=== AssignMembersToProject Started ===");
            Console.WriteLine($"Project name: '{projectName}'");
            Console.WriteLine($"Members to add: {membersDTO?.Count ?? 0}");

            if (string.IsNullOrEmpty(projectName))
            {
                throw new ArgumentException("Project name cannot be null or empty");
            }

            if (membersDTO == null || !membersDTO.Any())
            {
                throw new ArgumentException("No members provided");
            }

            CheckAdminProyectRole();
            Console.WriteLine("Admin role check passed");

            Project project = _repositoryManager.ProjectRepository.Get(p => p.Name == projectName);
            if (project == null)
            {
                Console.WriteLine($"Project '{projectName}' not found");
                throw new ProjectNotFoundException();
            }

            Console.WriteLine($"Project found: {project.Name} (ID: {project.Id})");
            Console.WriteLine($"Current members: {project.Members?.Count ?? 0}");

            // Inicializar Members si es null
            if (project.Members == null)
            {
                project.Members = new List<User>();
            }

            foreach (UserDTO memberDTO in membersDTO)
            {
                Console.WriteLine($"Processing member: {memberDTO.FirstName} {memberDTO.LastName} ({memberDTO.Email})");

                // CAMBIO: Lanzar excepción si ya es miembro (en lugar de skip)
                if (project.Members.Any(u => u.Email == memberDTO.Email))
                {
                    Console.WriteLine($"User {memberDTO.Email} is already a member - throwing exception");
                    throw new UserIsAlreadyAMemberException();
                }

                // Buscar el usuario en la base de datos
                User existingUser = _repositoryManager.UserRepository.Get(u => u.Email == memberDTO.Email);
                if (existingUser == null)
                {
                    Console.WriteLine($"User {memberDTO.Email} not found in database");
                    throw new UserNotFoundException();
                }

                Console.WriteLine(
                    $"User found in DB: {existingUser.FirstName} {existingUser.LastName} (ID: {existingUser.Id})");

                // Verificar que no esté ya en la lista por ID también
                if (project.Members.Any(u => u.Id == existingUser.Id))
                {
                    Console.WriteLine($"User {memberDTO.Email} already exists by ID - throwing exception");
                    throw new UserIsAlreadyAMemberException();
                }

                // Agregar el usuario encontrado en la DB
                project.Members.Add(existingUser);
                Console.WriteLine($"User {memberDTO.Email} added successfully");
            }

            Console.WriteLine("Saving changes...");
            _repositoryManager.ProjectRepository.Update(project);
            Console.WriteLine("Changes saved successfully");

            Console.WriteLine("=== AssignMembersToProject Completed Successfully ===");
        }
        catch (ProjectNotFoundException pnfEx)
        {
            Console.WriteLine($"ProjectNotFoundException: {pnfEx.Message}");
            throw;
        }
        catch (UserNotFoundException unfEx)
        {
            Console.WriteLine($"UserNotFoundException: {unfEx.Message}");
            throw;
        }
        catch (UserIsAlreadyAMemberException uamEx)
        {
            Console.WriteLine($"UserIsAlreadyAMemberException: {uamEx.Message}");
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error in AssignMembersToProject:");
            Console.WriteLine($"Type: {ex.GetType().Name}");
            Console.WriteLine($"Message: {ex.Message}");
            Console.WriteLine($"Inner: {ex.InnerException?.Message}");
            Console.WriteLine($"Stack: {ex.StackTrace}");
            throw new Exception($"Failed to assign members to project: {ex.Message}", ex);
        }
    }

    public void RemoveMemberFromProject(string projectName, string memberEmail)
    {
        CheckAdminProyectRole();
        Project project = _repositoryManager.ProjectRepository.Get(p => p.Name == projectName);
        if (project == null) throw new ProjectNotFoundException();

        if (project.Members.Find(m => m.Email == memberEmail) == null) throw new UserIsNotAMemberException();

        project.Members.Remove(project.Members.Find(m => m.Email == memberEmail));
        _repositoryManager.ProjectRepository.Update(project);
    }

    public void RemoveAdminFromProject(string projectName, string adminEmail)
    {
        CheckAdminProyectRole();
        Project project = _repositoryManager.ProjectRepository.Get(p => p.Name == projectName);
        if (project == null) throw new ProjectNotFoundException();

        if (project.AdminProject == null || project.AdminProject.Email != adminEmail)
            throw new UserIsNotAMemberException();

        project.AdminProject = null;
        _repositoryManager.ProjectRepository.Update(project);
    }


    public void RemoveProject(string projectName)
    {
        CheckAdminProyectRole();
        Project project = _repositoryManager.ProjectRepository.Get(p => p.Name == projectName);
        _repositoryManager.ProjectRepository.Delete(project);
    }

    public void UpdateProject(string projectNameToUpdate, ProjectDTO updatedProjectDTO)
    {
        CheckAdminProyectRole();
        Project existingProject = _repositoryManager.ProjectRepository.Get(p => p.Name == projectNameToUpdate);
        if (existingProject == null) throw new ProjectNotFoundException();

        Project updatedProject = _projectConverter.ToEntity(updatedProjectDTO);

        updatedProject.Id = existingProject.Id;
        SetProjectAdmin(updatedProject, updatedProjectDTO);

        _repositoryManager.ProjectRepository.Update(updatedProject);
    }

    public List<ProjectDTO> GetAllProjects()
    {
        CheckAdminProyectRole();
        List<Project> projects = _repositoryManager.ProjectRepository.GetAll();
        List<ProjectDTO> projectDTOs = new List<ProjectDTO>();
        UserDTO loggedUser = LoggedUser.Current;
        foreach (Project project in projects)
        {
            if (project.AdminProject.Email == loggedUser.Email)
            {
                projectDTOs.Add(_projectConverter.FromEntity(project));
            }
        }

        return projectDTOs;
    }

    public ProjectDTO GetProjectByName(string projectName)
    {
        Project project = _repositoryManager.ProjectRepository.Get(p => p.Name == projectName);
        if (project == null) throw new ProjectNotFoundException();

        return _projectConverter.FromEntity(project);
    }

    public List<ProjectDTO> GetAllProjectsForUser(string Email)
    {
        List<ProjectDTO> projects = new List<ProjectDTO>();

        foreach (Project project in _repositoryManager.ProjectRepository.GetAll())
            if (project.AdminProject.Email == Email || project.Members.Any(m => m.Email == Email))
                projects.Add(_projectConverter.FromEntity(project));

        return projects;
    }

    public List<UserDTO> GetMembers(string projectName)
    {
        CheckAdminProyectRole();
        ProjectDTO project = GetProjectByName(projectName);
        if (project == null) throw new ProjectNotFoundException();

        List<UserDTO> memberDTOs = project.Members;

        return memberDTOs;
    }

    public void AddTaskToMember(string projectName, string memberEmail, string title)
    {
        CheckAdminProyectRole();
        Project projectEntity = _repositoryManager.ProjectRepository.Get(p => p.Name == projectName);
        if (projectEntity == null) throw new ProjectNotFoundException();

        if (projectEntity.Members == null || !projectEntity.Members.Any(m => m.Email == memberEmail))
            throw new UserIsNotAMemberException();

        if (projectEntity.Tasks == null || !projectEntity.Tasks.Any(t => t.Title == title))
            throw new TaskIsNotFromTheProjectException();

        User userEntity = _repositoryManager.UserRepository.Get(u => u.Email == memberEmail);
        if (userEntity == null) throw new UserNotFoundException();

        Task task = _repositoryManager.TaskRepository.Get(t => t.Title == title);
        userEntity.Tasks.Add(task);
        _repositoryManager.UserRepository.Update(userEntity);
    }

    public void RemoveTaskFromMember(string projectName, string memberEmail, string title)
    {
        CheckAdminProyectRole();
        Project projectEntity = _repositoryManager.ProjectRepository.Get(p => p.Name == projectName);
        if (projectEntity == null) throw new ProjectNotFoundException();

        if (projectEntity.Members == null || !projectEntity.Members.Any(m => m.Email == memberEmail))
            throw new UserIsNotAMemberException();

        if (projectEntity.Tasks == null || !projectEntity.Tasks.Any(t => t.Title == title))
            throw new TaskIsNotFromTheProjectException();

        User userEntity = _repositoryManager.UserRepository.Get(u => u.Email == memberEmail);
        if (userEntity == null) throw new UserNotFoundException();

        Task task = projectEntity.Tasks.Find(t => t.Title == title);
        userEntity.RemoveTask(task);
        _repositoryManager.UserRepository.Update(userEntity);
    }

    public List<TaskDTO> GetAllTaskForAMember(string email)
    {
        User user = _repositoryManager.UserRepository.Get(u => u.Email == email);
        CpmService cpmService = new CpmService();
        TaskService taskService = new TaskService(_repositoryManager, cpmService);
        List<TaskDTO> returnList = new List<TaskDTO>();
        foreach (Project project in _repositoryManager.ProjectRepository.GetAll())
        {
            List<TaskDTO> tasks = taskService.GetTasks(project.Name);
            foreach (TaskDTO task in tasks)
                if (task.Id.HasValue && user.Tasks.Any(t => t.Id == task.Id))
                    returnList.Add(task);
        }

        return returnList;
    }


    public List<TaskDTO> GetAllTaskForAMemberInAProject(string projectName, string email)
    {
        User user = _repositoryManager.UserRepository.Get(u => u.Email == email);
        if (user == null) throw new UserNotFoundException();
        if (user.Tasks == null) return new List<TaskDTO>();
        Console.WriteLine($"{user.Email}", ConsoleColor.Yellow);
        CpmService cpmService = new CpmService();
        TaskService taskService =
            new TaskService(_repositoryManager, cpmService);

        List<TaskDTO> returnList = new List<TaskDTO>();
        List<TaskDTO> tasks = taskService.GetTasks(projectName);
        foreach (TaskDTO task in tasks)
        {
            if (task.Id.HasValue && user.Tasks.Any((t => t.Id == task.Id)))
            {
                returnList.Add(task);
            }
        }

        return returnList;
    }


    private void CheckAdminProyectRole()
    {
        UserDTO currentUser = LoggedUser.Current;
        if (currentUser == null || !currentUser.Roles.Contains(RolDTO.AdminProject))
            throw new UnauthorizedAdminAccessException();
    }

    private void SetProjectAdmin(Project project, ProjectDTO projectDTO)
    {
        if (projectDTO.AdminProyect != null)
        {
            User adminFromDb = _repositoryManager.UserRepository.Get(u => u.Email == projectDTO.AdminProyect.Email);
            project.AdminProject = adminFromDb;
        }
        else
        {
            project.AdminProject = _repositoryManager.UserRepository.Get(u => u.Email == LoggedUser.Current.Email);
        }
    }
}