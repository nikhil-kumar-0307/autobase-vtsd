// Models/Vehicle.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace autobase.Models
{
    public class Vehicle
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int VehicleTypeId { get; set; }

        [ForeignKey("VehicleTypeId")]
        public virtual VehicleTypes VehicleTypeRef { get; set; }

        [Required, MaxLength(100)]
        public string VehicleName { get; set; }

        [Required, MaxLength(50)]
        public string VehicleType { get; set; }

        [Required, MaxLength(20)]
        public string RegistrationNo { get; set; }

        [Required]
        public int YearOfManufacture { get; set; }

        // "Available", "Allocated", "Maintenance"
        [Required, MaxLength(20)]
        public string Status { get; set; } = "Available";

        [MaxLength(500)]
        public string Notes { get; set; }

        [MaxLength(20)]
        public string FuelType { get; set; }

        public int SeatingCapacity { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int CreatedBy { get; set; }
    }
}