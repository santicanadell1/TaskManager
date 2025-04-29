namespace Domain.Exceptions;

public class UserRolesInvalidAssignmentException : UserException
{
    public UserRolesInvalidAssignmentException() 
        : base("Roles cannot be assigned as null.") { }
}