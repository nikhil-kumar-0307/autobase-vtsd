using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace autobase.Models
{
    [Table("VehicleRequests")]
    public class VehicleRequest
    {
        [Key]
        public int RequestId { get; set; }

        // Which vehicle
        public int VehicleId { get; set; }
        public string VehicleName { get; set; }
        public string RegistrationNo { get; set; }

        // Who is requesting
        public string EmployeeNumber { get; set; }

        // Request details
        [Required]
        public string Purpose { get; set; }

        [Required]
        public DateTime RequiredFrom { get; set; }

        [Required]
        public DateTime RequiredUntil { get; set; }

        // Status
        public string Status { get; set; }   // Pending / Approved / Rejected

        public string AdminNotes { get; set; }
        public DateTime RequestedOn { get; set; }
    }
}
