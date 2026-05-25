using System;
using System.Linq;
using System.Web.Mvc;
using autobase.Data;
using autobase.Filters;
using autobase.Models.DTOs;

namespace autobase.Controllers
{
    [Authorize]
    [SessionRestore]
    public class DashboardController : Controller
    {
        private AutobaseDbContext db = new AutobaseDbContext();

        [HttpGet]
        public ActionResult Index()
        {
            string role = Session["Role"]?.ToString();
            if (role == "Employee")
                return RedirectToAction("Index", "EmployeeDashboard");

            var now = DateTime.Now;
            var dto = new AdminDashboardDto();

            
            var vehicles = db.Vehicles.ToList();

            dto.TotalVehicles = vehicles.Count();
            dto.Available = vehicles.Count(v => v.Status == "Available");
            dto.Allocated = vehicles.Count(v => v.Status == "Allocated");
            dto.Maintenance = vehicles.Count(v => v.Status == "Maintenance");

            dto.RecentVehicles = vehicles
                .Where(v => v.IsActive)
                .OrderByDescending(v => v.Id)
                .Take(6)
                .Select(v => new DashboardVehicleRow
                {
                    Id = v.Id,
                    VehicleName = v.VehicleName,      // direct column
                    VehicleType = v.VehicleType,      // direct column
                    RegistrationNo = v.RegistrationNo,
                    Status = v.Status,
                    YearOfManufacture = v.YearOfManufacture
                }).ToList();

            // ── Employees ────────────────────────────────────────
            var employees = db.Employees.ToList();

            dto.TotalUsers = employees.Count();
            dto.TotalAdmins = employees.Count(e => e.Role == "Admin" || e.Role == "SuperAdmin");
            dto.TotalEmployees = employees.Count(e => e.Role == "Employee");

            dto.RecentUsers = employees
                .OrderByDescending(e => e.Id)
                .Take(6)
                .Select(e => new DashboardUserRow
                {
                    Id = e.Id,
                    FullName = e.FullName,
                    EmployeeNumber = e.EmployeeNumber,
                    Role = e.Role,
                    Designation = e.Designation,
                    Department = e.Department
                }).ToList();

            
            var requests = db.VehicleRequests.ToList();

            dto.TotalRequests = requests.Count();
            dto.PendingRequests = requests.Count(r => r.Status == "Pending");
            dto.ApprovedRequests = requests.Count(r => r.Status == "Approved");
            dto.RejectedRequests = requests.Count(r => r.Status == "Rejected");
            dto.CompletedRequests = requests.Count(r => r.Status == "Returned");

            
            var empLookup = employees.ToDictionary(
                e => e.EmployeeNumber,
                e => e.FullName
            );

            dto.RecentRequests = requests
                .OrderByDescending(r => r.RequestedOn)
                .Take(5)
                .Select(r => new DashboardRequestRow
                {
                    RequestId = r.RequestId,
                    EmployeeNumber = r.EmployeeNumber,
                    EmployeeName = empLookup.ContainsKey(r.EmployeeNumber)
                                        ? empLookup[r.EmployeeNumber]
                                        : r.EmployeeNumber,   // fallback
                    VehicleName = r.VehicleName,
                    RegistrationNo = r.RegistrationNo,
                    Status = r.Status,
                    Purpose = r.Purpose,
                    RequestedOn = r.RequestedOn,
                    RequiredFrom = r.RequiredFrom,
                    RequiredUntil = r.RequiredUntil,
                    IsOverdue = r.Status == "Approved" && r.RequiredUntil < now
                }).ToList();

            return View(dto);
        }

        [HttpGet]
        public ActionResult EmployeeDashboard()
        {
            return RedirectToAction("Index", "EmployeeDashboard");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}