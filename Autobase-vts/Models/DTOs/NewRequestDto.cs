using System;
using System.Collections.Generic;

namespace autobase.Models.DTOs
{
    public class NewRequestDto
    {
        public IEnumerable<AvailableVehicleItem> AvailableVehicles { get; set; }
            = new List<AvailableVehicleItem>();
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

        // ── NEW ──
        public bool IsAvailable { get; set; } = true;
        public DateTime? FreeAt { get; set; }

        // How long until it's free — calculated property
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
