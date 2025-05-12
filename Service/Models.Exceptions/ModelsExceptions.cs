namespace Service.Models.Exceptions
{
    public class ModelsExceptions : Exception
    {
        public ModelsExceptions(string message) : base(message)
        {
        }

        public override string ToString()
        {
            return $"ModelsExceptions: {this.GetType().Name} - {this.Message}";
        }
    }
}