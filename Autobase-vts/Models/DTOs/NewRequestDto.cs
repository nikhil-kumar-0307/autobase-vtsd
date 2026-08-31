using System;
using System.Collections.Generic;
namespace autobase.Models.DTOs
{
    public class NewRequestDto
    {
        public IEnumerable<AvailableVehicleItem> AvailableVehicles { get; set; }
            = new List<AvailableVehicleItem>();

        // ── pagination ──
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalVehicleCount { get; set; }

        // ── full-fleet counts (not just current page/filter) ──
        public int TotalAvailable { get; set; }
        public int TotalInUse { get; set; }
        public int TotalMaintenance { get; set; }

        // ── NEW: which status tab is currently active ──
        public string ActiveFilter { get; set; } = "All";
    }

    public class AvailableVehicleItem
    {
        public int VehicleId { get; set; }
        public string VehicleName { get; set; }
        public string RegistrationNo { get; set; }
        public string VehicleTypeName { get; set; }
        public int YearOfManufacture { get; set; }
        public string FuelType { get; set; }
        public int SeatingCapacity { get; set; }
        public string Notes { get; set; }
        public bool IsAvailable { get; set; } = true;
        public DateTime? FreeAt { get; set; }
        public string Status { get; set; } = "Available";
        public bool IsMaintenance => Status == "Maintenance";

        public string FreeInLabel
        {
            get
            {
                if (IsAvailable || FreeAt == null) return null;
                var diff = FreeAt.Value - DateTime.Now;
                if (diff.TotalMinutes <= 0) return "Soon";
                if (diff.TotalHours < 1) return (int)diff.TotalMinutes + " min";
                if (diff.TotalHours < 24) return diff.Hours + "h " + diff.Minutes + "m";
                return (int)diff.TotalDays + " day" + ((int)diff.TotalDays > 1 ? "s" : "");
            }
        }
    }
}