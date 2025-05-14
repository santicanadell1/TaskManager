using System.Runtime.Intrinsics.Arm;

namespace Domain.Exceptions.NotificationExceptions;

public class NoResourcesFoundException : ResourceServiceException
{
    public NoResourcesFoundException() : base("No resources found")
    {
    }
}