using System.ComponentModel.DataAnnotations;

namespace autobase.Models.DTOs
{
    public class AddUserDto
    {
        [Required(ErrorMessage = "Full name is required.")]
        [MaxLength(100, ErrorMessage = "Max 100 characters.")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Employee number is required.")]
        [MaxLength(20, ErrorMessage = "Max 20 characters.")]
        [Display(Name = "Employee Number")]
        public string EmployeeNumber { get; set; }

        [Required(ErrorMessage = "Mobile number is required.")]
        [MaxLength(15, ErrorMessage = "Max 15 characters.")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Enter a valid 10-digit mobile number.")]
        [Display(Name = "Mobile Number")]
        public string MobileNumber { get; set; }

        [Required(ErrorMessage = "Designation is required.")]
        [MaxLength(100)]
        public string Designation { get; set; }

        [Required(ErrorMessage = "Department is required.")]
        [MaxLength(100)]
        public string Department { get; set; }

        [Required(ErrorMessage = "Please assign a role.")]
        public string Role { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [MaxLength(150)]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string Email { get; set; }
    }
}
