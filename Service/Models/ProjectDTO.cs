using System;
using System.ComponentModel.DataAnnotations;

namespace Service.Models
{
    public class ProjectDTO
    {
        [Required(ErrorMessage = "Project name is required.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Project description is required.")]
        public string Description { get; set; }

        public DateTime StartDate { get; set; }
    }
}