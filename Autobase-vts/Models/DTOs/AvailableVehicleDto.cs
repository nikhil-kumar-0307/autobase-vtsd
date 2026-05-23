using System.Collections.Generic;
using autobase.Models;

namespace autobase.Models.DTOs

{
    public class AvailableVehicleDto
    {
        public int TotalFleet { get; set; }
        public int TotalAvailable { get; set; }
        public int TotalInUse { get; set; }
        public int TotalMaintenance { get; set; }

        public List<VehicleTypeGroup> Groups { get; set; } = new List<VehicleTypeGroup>();

        public string ActiveFilter { get; set; } = "All";
        public string SearchTerm { get; set; } = "";
    }

    public class VehicleTypeGroup
    {
        public string TypeName { get; set; }
        public int Total { get; set; }
        public int Available { get; set; }
        public int InUse { get; set; }
        public int Maintenance { get; set; }
        public int Percent { get; set; }

        public List<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
    }
}