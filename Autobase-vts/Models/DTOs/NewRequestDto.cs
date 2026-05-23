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
    }
}
