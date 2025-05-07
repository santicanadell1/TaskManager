using System;
using System.ComponentModel.DataAnnotations;

namespace Service.Models
{
    public class ProjectDTO
    {
        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; }

        public string Description { get; set; }

        public DateTime StartDate { get; set; }
    }
}