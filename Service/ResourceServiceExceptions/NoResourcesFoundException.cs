using System.Runtime.Intrinsics.Arm;

namespace Domain.Exceptions;

public class NoResourcesFoundException : ResourceServiceException
{
    public NoResourcesFoundException() : base("No resources found")
    {
    }
}