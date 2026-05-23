using System;
using System.Collections.Generic;

namespace autobase.Models.DTOs
{
    public class MyRequestDto
    {
        public List<MyRequestItem> Requests { get; set; } = new List<MyRequestItem>();
    }

    public class MyRequestItem
    {
        public int RequestId { get; set; }
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
    }
}