
using System.ComponentModel.DataAnnotations;

namespace autobase.Models.DTOs
{
    public class AddVehicleDto
    {
        [Required(ErrorMessage = "Please select a vehicle name.")]
        public int VehicleTypeId { get; set; }

        [Required(ErrorMessage = "Registration number is required.")]
        [MaxLength(20, ErrorMessage = "Max 20 characters.")]
        [Display(Name = "Registration No")]
        public string RegistrationNo { get; set; }

        [Required(ErrorMessage = "Year of manufacture is required.")]
        [Range(1990, 2030, ErrorMessage = "Year must be between 1990 and 2030.")]
        [Display(Name = "Year of Manufacture")]
        public int? YearOfManufacture { get; set; }

        [Required(ErrorMessage = "Please select a status.")]
        public string Status { get; set; } = "Available";

        [MaxLength(500)]
        public string Notes { get; set; }
    }
}
