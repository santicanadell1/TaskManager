using Domain.Exceptions;

namespace Domain
{
    public class Resource
    {
        public string name;
        public string type;
        public string description;

        public string Name
        {
            get => name;
            set { name = string.IsNullOrEmpty(value) ? throw new ResourceNameException() : value; }
        }

        public string Type
        {
            get => type;
            set { type = string.IsNullOrEmpty(value) ? throw new ResourceTypeException() : value; }
        }

        public string Description { get; set; }

        public int? Id { get; set; }

        public Resource(int? id, string name, string type, string description)
        {
            this.Id = id;
            this.Name = name;
            this.Type = type;
            this.Description = description;
        }
    }
}