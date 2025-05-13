namespace DataAccess.Exceptions.UserRepositoryExceptions
{
    public class UserRepositoryExceptions : Exception
    {
        public UserRepositoryExceptions(string message) 
            : base(message) { }

        public override string ToString()
        {
            return $"UserRepositoryExceptions: {this.GetType().Name} - {this.Message}";
        }
    }
}