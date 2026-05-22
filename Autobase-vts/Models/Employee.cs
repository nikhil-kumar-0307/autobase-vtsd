using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace autobase.Models
{
    public class Employee
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(20)]
        public string EmployeeNumber { get; set; }

        [Required, MaxLength(100)]
        public string FullName { get; set; }

        [MaxLength(150)]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        // Roles: "SuperAdmin", "Admin", "Employee"
        [Required, MaxLength(20)]
        public string Role { get; set; }
        public string MobileNumber { get; set; }
        public string Designation { get; set; }
        public string Department { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? LastLoginAt { get; set; }
    }
}