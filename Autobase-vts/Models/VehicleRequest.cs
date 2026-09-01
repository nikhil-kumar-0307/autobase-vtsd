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

        public int VehicleId { get; set; }
        public string VehicleName { get; set; }
        public string RegistrationNo { get; set; }

        public string EmployeeNumber { get; set; }

        [Required]
        public string Purpose { get; set; }
        [Required]
        public DateTime RequiredFrom { get; set; }
        [Required]
        public DateTime RequiredUntil { get; set; }

        // Status: Pending -> HODApproved -> Approved / Rejected / Returned
        public string Status { get; set; }

        public string AdminNotes { get; set; }     // final (Admin/SuperAdmin) notes
        public DateTime RequestedOn { get; set; }

        // ── NEW: HOD (first-stage) approval tracking ──
        public string HodNotes { get; set; }
        public string HodApprovedBy { get; set; }
        public DateTime? HodApprovedOn { get; set; }

        // ── NEW: final (second-stage) approval tracking ──
        public string FinalApprovedBy { get; set; }
        public DateTime? FinalApprovedOn { get; set; }
    }
}