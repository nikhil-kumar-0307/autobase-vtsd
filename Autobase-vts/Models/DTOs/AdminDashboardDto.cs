using System;
using System.Collections.Generic;

namespace autobase.Models.DTOs
{
    public class AdminDashboardDto
    {
        // ── Fleet Stats ───────────────────────────────────────────
        public int TotalVehicles { get; set; }
        public int Available { get; set; }   
        public int Allocated { get; set; }   
        public int Maintenance { get; set; }   

        // ── User Stats ────────────────────────────────────────────
        public int TotalUsers { get; set; }
        public int TotalAdmins { get; set; }
        public int TotalEmployees { get; set; }

        // ── Request Stats ─────────────────────────────────────────
        public int TotalRequests { get; set; }
        public int PendingRequests { get; set; }
        public int ApprovedRequests { get; set; }
        public int RejectedRequests { get; set; }
        public int CompletedRequests { get; set; }

        // ── Utilisation % ─────────────────────────────────────────
        public int UtilisationPercent =>
            TotalVehicles > 0
                ? (int)Math.Round(Allocated * 100.0 / TotalVehicles)
                : 0;

        // ── Recent Records ────────────────────────────────────────
        public List<DashboardVehicleRow> RecentVehicles { get; set; } = new List<DashboardVehicleRow>();
        public List<DashboardUserRow> RecentUsers { get; set; } = new List<DashboardUserRow>();
        public List<DashboardRequestRow> RecentRequests { get; set; } = new List<DashboardRequestRow>();
    }

    public class DashboardVehicleRow
    {
        public int Id { get; set; }
        public string VehicleName { get; set; }
        public string VehicleType { get; set; }
        public string RegistrationNo { get; set; }
        public string Status { get; set; }
        public int YearOfManufacture { get; set; }
    }

    public class DashboardUserRow
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string EmployeeNumber { get; set; }
        public string Role { get; set; }
        public string Designation { get; set; }
        public string Department { get; set; }
    }

    public class DashboardRequestRow
    {
        public int RequestId { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeNumber { get; set; }
        public string VehicleName { get; set; }
        public string RegistrationNo { get; set; }
        public string Status { get; set; }
        public string Purpose { get; set; }
        public DateTime RequestedOn { get; set; }
        public DateTime RequiredFrom { get; set; }
        public DateTime RequiredUntil { get; set; }
        public bool IsOverdue { get; set; }
    }
}