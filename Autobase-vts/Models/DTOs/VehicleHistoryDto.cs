using System;
using System.Collections.Generic;

namespace autobase.Models.DTOs
{
    public class VehicleHistoryDto
    {
        public string SearchTerm { get; set; } = "";
        public bool HasSearched { get; set; }

        // "All" | "Today" | "Last5" | "Last7" | "Last30" | "Custom"
        public string Preset { get; set; } = "All";
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalItems { get; set; }
        public int TotalPages => TotalItems == 0 ? 1 : (int)Math.Ceiling(TotalItems / (double)PageSize);

        // ── LIST MODE (no search) — all vehicles, most recently added first ──
        public List<VehicleListItem> Vehicles { get; set; } = new List<VehicleListItem>();

        // ── DETAIL MODE (search performed) — full trip history for matched vehicle(s) ──
        public int TotalTrips { get; set; }
        public double TotalHours { get; set; }
        public DateTime? FirstUsedOn { get; set; }
        public DateTime? LastUsedOn { get; set; }
        public int DistinctVehiclesMatched { get; set; }
        public List<VehicleHistoryItem> Items { get; set; } = new List<VehicleHistoryItem>();
    }

    public class VehicleListItem
    {
        public int VehicleId { get; set; }
        public string VehicleName { get; set; }
        public string RegistrationNo { get; set; }
        public string VehicleType { get; set; }
        public string Status { get; set; }
        public DateTime AddedOn { get; set; }
        public int TripCount { get; set; }
        public DateTime? LastUsedOn { get; set; }
    }

    public class VehicleHistoryItem
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
        public DateTime RequestedOn { get; set; }

        public string Status { get; set; }

        public string AdminNotes { get; set; }
        public string HodNotes { get; set; }
        public string HodApprovedBy { get; set; }

        public double DurationHours => (RequiredUntil - RequiredFrom).TotalHours;
    }
}