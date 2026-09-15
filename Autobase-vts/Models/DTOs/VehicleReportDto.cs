using System;
using System.Collections.Generic;

namespace autobase.Models.DTOs
{
    public class VehicleReportDto
    {
        // "Today" | "Last5" | "Last7" | "Last30" | "Custom"
        public string Preset { get; set; } = "Last5";

        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public string SearchTerm { get; set; } = "";

        // Top summary numbers
        public int TotalTrips { get; set; }
        public int DistinctVehiclesUsed { get; set; }
        public double TotalHoursUsed { get; set; }

        // Per-vehicle rollup ("which vehicle is used" summary)
        public List<VehicleUsageSummary> VehicleSummaries { get; set; } = new List<VehicleUsageSummary>();

        // Trip-level detail rows
        public List<VehicleReportItem> Items { get; set; } = new List<VehicleReportItem>();
    }

    public class VehicleUsageSummary
    {
        public int VehicleId { get; set; }
        public string VehicleName { get; set; }
        public string RegistrationNo { get; set; }
        public int TripCount { get; set; }
        public double TotalHours { get; set; }
        public DateTime LastUsedOn { get; set; }
    }

    public class VehicleReportItem
    {
        public int RequestId { get; set; }
        public int VehicleId { get; set; }
        public string VehicleName { get; set; }
        public string RegistrationNo { get; set; }

        public string EmployeeName { get; set; }
        public string EmployeeNumber { get; set; }
        public string Department { get; set; }

        public string Purpose { get; set; }
        public string ReportingPlace { get; set; }

        public DateTime RequiredFrom { get; set; }
        public DateTime RequiredUntil { get; set; }

        public string Status { get; set; }

        // Hours of usage that actually fall inside the selected report range
        // (clamped so a trip that starts before the range only counts the overlap)
        public double UsageHoursInRange { get; set; }

        public double DurationHours => (RequiredUntil - RequiredFrom).TotalHours;
    }
}
