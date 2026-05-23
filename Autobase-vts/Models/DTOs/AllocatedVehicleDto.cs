using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Autobase_vts.Models.DTOs
{
    public class AllocatedVehicleDto
    {
        public int TotalAllocated { get; set; }
        public int InUseCount { get; set; }   // within return time
        public int OverdueCount { get; set; }   // past due return time

        public string ActiveFilter { get; set; } = "All";
        public string SearchTerm { get; set; } = "";

        public List<AllocatedVehicleItem> Items { get; set; } = new List<AllocatedVehicleItem>();
    }

    public class AllocatedVehicleItem
    {
        public int RequestId { get; set; }
        public int VehicleId { get; set; }
        public string VehicleName { get; set; }
        public string VehicleType { get; set; }
        public string RegistrationNo { get; set; }

        // Employee info
        public string EmployeeName { get; set; }
        public string EmployeeNumber { get; set; }
        public string EmployeePhone { get; set; }
        public string Initials { get; set; }

        // Request info
        public DateTime StartTime { get; set; }
        public DateTime DueReturn { get; set; }
        public string Purpose { get; set; }

        // Computed
        public bool IsOverdue { get; set; }
        public TimeSpan OverdueBy { get; set; }
        public int DurationHours { get; set; }
        public int DurationMins { get; set; }
        public int OverduePercent { get; set; }  // 0-100 for progress bar
    }
}