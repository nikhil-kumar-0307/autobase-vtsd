// Models/DTOs/VehicleTypesDto.cs
using System.ComponentModel.DataAnnotations;

namespace autobase.Models.DTOs
{
    public class VehicleTypesDto
    {
        [Required(ErrorMessage = "Vehicle name is required.")]
        [MaxLength(100, ErrorMessage = "Vehicle name cannot exceed 100 characters.")]
        [Display(Name = "Vehicle Name")]
        public string VehicleName { get; set; }

        [Required(ErrorMessage = "Vehicle type is required.")]
        [MaxLength(50)]
        [Display(Name = "Vehicle Type")]
        public string VehicleType { get; set; }
    }
}
