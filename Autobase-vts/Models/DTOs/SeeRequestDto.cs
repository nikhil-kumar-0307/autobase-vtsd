using System;
using System.Collections.Generic;

namespace autobase.Models.DTOs
{
    public class SeeRequestDto
    {
        public int TotalCount { get; set; }
        public int PendingCount { get; set; }
        public int ApprovedCount { get; set; }
        public int RejectedCount { get; set; }
        public int CompletedCount { get; set; }

        public string ActiveFilter { get; set; } = "All";
        public string SearchTerm { get; set; } = "";

        public List<SeeRequestItem> Requests { get; set; } = new List<SeeRequestItem>();
    }

    public class SeeRequestItem
    {
        public int RequestId { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeNumber { get; set; }
        public string EmployeePhone { get; set; }
        public string Department { get; set; }
        public string Designation { get; set; }
        public string VehicleName { get; set; }
        public string RegistrationNo { get; set; }
        public string Purpose { get; set; }
        public DateTime RequiredFrom { get; set; }
        public DateTime RequiredUntil { get; set; }
        public string Status { get; set; }
        public string AdminNotes { get; set; }
        public DateTime RequestedOn { get; set; }

        public double DurationHours =>
            (RequiredUntil - RequiredFrom).TotalHours;

        public bool IsOverdue =>
            Status == "Approved" && RequiredUntil < DateTime.Now;
    }
}