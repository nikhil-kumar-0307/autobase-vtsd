using Autobase_vts.Models.DTOs;
using System;
using System.Collections.Generic;

namespace autobase.Models.DTOs
{
    public class EmployeeDashboardDto
    {
        // Stats
        public int TotalRequests { get; set; }
        public int ApprovedCount { get; set; }
        public int PendingCount { get; set; }
        public int RejectedCount { get; set; }
        public int AllocatedCount { get; set; }  // currently approved / in use

        // Recent requests (latest 5)
        public List<MyRequestItem> RecentRequests { get; set; } = new List<MyRequestItem>();

        // Currently allocated vehicles
        public List<AllocatedVehicleItem> AllocatedVehicles { get; set; } = new List<AllocatedVehicleItem>();
    }
}