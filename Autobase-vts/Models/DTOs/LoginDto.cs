// Models/DTOs/LoginDto.cs
using System.ComponentModel.DataAnnotations;

namespace autobase.Models.DTOs
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Employee number is required.")]
        [Display(Name = "Employee Number")]
        public string EmployeeNumber { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}