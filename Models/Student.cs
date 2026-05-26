using System.ComponentModel.DataAnnotations;

namespace StudentManagementSystem.Models
{
    public class Student
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Range(1, 100)]
        public int Age { get; set; }

        [Required]
        [MaxLength(100)]
        public string Course { get; set; } = string.Empty;

        [Required]
        [MaxLength(15)]
        public string Phone { get; set; } = string.Empty;
    }
}