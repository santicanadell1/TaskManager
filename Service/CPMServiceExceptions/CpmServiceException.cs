using System;

namespace Domain.Exceptions
{
    public class CpmServiceException : Exception
    {
        public CpmServiceException(string message) : base(message) { }

        public override string ToString()
        {
            return $"CpmServiceException: {this.GetType().Name} - {this.Message}";
        }
    }
}