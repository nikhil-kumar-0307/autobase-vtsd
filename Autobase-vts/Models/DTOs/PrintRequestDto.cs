// Models/DTOs/PrintRequestDto.cs
namespace autobase.Models.DTOs
{
    public class PrintRequestDto
    {
        // Employee info
        public string EmployeeName { get; set; }
        public string EmployeeNumber { get; set; }
        public string EmployeePhone { get; set; }
        public string Designation { get; set; }
        public string Department { get; set; }

        // Vehicle info
        public string VehicleName { get; set; }
        public string RegistrationNo { get; set; }

        // Booking info
        public System.DateTime RequiredFrom { get; set; }
        public System.DateTime RequiredUntil { get; set; }
        public double DurationHours { get; set; }
        public string Purpose { get; set; }

        // Meta
        public string AdminNotes { get; set; }
        public System.DateTime RequestedOn { get; set; }
        public string Status { get; set; }
        public int RequestId { get; set; }

        // Computed helpers (used in the view)
        public string DurationLabel =>
            DurationHours >= 1
                ? DurationHours.ToString("0.#") + " hrs"
                : ((int)(DurationHours * 60)) + " min";
    }
}