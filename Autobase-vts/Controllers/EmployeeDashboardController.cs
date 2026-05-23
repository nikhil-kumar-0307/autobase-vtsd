using System;
using System.Linq;
using System.Web.Mvc;
using autobase.Data;
using autobase.Models;
using autobase.Models.DTOs;

namespace autobase.Controllers
{
    [Authorize]
    public class EmployeeDashboardController : Controller
    {
        private readonly AutobaseDbContext _db = new AutobaseDbContext();

        // ── GET: /EmployeeDashboard/Index ────────────────────────────────────
        [HttpGet]
        public ActionResult Index()
        {
            string role = Session["Role"]?.ToString();
            if (role != "Employee")
                return RedirectToAction("Login", "Account");

            return View("~/Views/Dashboard/EmployeeDashboard.cshtml");
        }

        // ── GET: /EmployeeDashboard/AvailableVehicles ────────────────────────
        [HttpGet]
        public ActionResult AvailableVehicles(string filter = "All", string search = "")
        {
            string role = Session["Role"]?.ToString();
            if (role != "Employee")
                return RedirectToAction("Login", "Account");

            var query = _db.Vehicles.Where(v => v.IsActive);

            if (!string.IsNullOrWhiteSpace(search))
            {
                string s = search.Trim().ToLower();
                query = query.Where(v =>
                    v.VehicleName.ToLower().Contains(s) ||
                    v.RegistrationNo.ToLower().Contains(s));
            }

            if (filter == "Available")
                query = query.Where(v => v.Status == "Available");
            else if (filter == "InUse" || filter == "Allocated")
                query = query.Where(v => v.Status == "Allocated");
            else if (filter == "Maintenance")
                query = query.Where(v => v.Status == "Maintenance");

            var filteredList = query
                .OrderBy(v => v.VehicleType)
                .ThenBy(v => v.VehicleName)
                .ToList();

            var allVehicles = _db.Vehicles.Where(v => v.IsActive).ToList();

            var dto = new AvailableVehicleDto
            {
                TotalFleet = allVehicles.Count,
                TotalAvailable = allVehicles.Count(v => v.Status == "Available"),
                TotalInUse = allVehicles.Count(v => v.Status == "Allocated"),
                TotalMaintenance = allVehicles.Count(v => v.Status == "Maintenance"),
                ActiveFilter = filter,
                SearchTerm = search,
                Groups = filteredList
                    .GroupBy(v => v.VehicleType)
                    .Select(g => new VehicleTypeGroup
                    {
                        TypeName = g.Key,
                        Total = g.Count(),
                        Available = g.Count(v => v.Status == "Available"),
                        InUse = g.Count(v => v.Status == "Allocated"),
                        Maintenance = g.Count(v => v.Status == "Maintenance"),
                        Percent = g.Count() > 0
                                        ? (int)((g.Count(v => v.Status == "Available") * 100.0) / g.Count())
                                        : 0,
                        Vehicles = g.OrderBy(v => v.VehicleName).ToList()
                    })
                    .OrderBy(g => g.TypeName)
                    .ToList()
            };

            return View(dto);
        }

        // ── GET: /EmployeeDashboard/NewRequest ───────────────────────────────
        [HttpGet]
        public ActionResult NewRequest()
        {
            string role = Session["Role"]?.ToString();
            if (role != "Employee")
                return RedirectToAction("Login", "Account");

            var vehicles = _db.Vehicles
                .Where(v => v.IsActive && (v.Status == "Available" || v.Status == "Allocated"))
                .OrderBy(v => v.VehicleType)
                .ThenBy(v => v.VehicleName)
                .ToList()
                .Select(v =>
                {
            // Find the active approved request for this vehicle
            var activeRequest = _db.VehicleRequests
                        .Where(r => r.VehicleId == v.Id && r.Status == "Approved")
                        .OrderByDescending(r => r.RequiredUntil)
                        .FirstOrDefault();

                    DateTime? freeAt = activeRequest?.RequiredUntil;

                    return new AvailableVehicleItem
                    {
                        VehicleId = v.Id,
                        VehicleName = v.VehicleName,
                        RegistrationNo = v.RegistrationNo,
                        VehicleTypeName = v.VehicleType,
                        YearOfManufacture = v.YearOfManufacture,
                        FuelType = "—",
                        SeatingCapacity = 0,
                        Notes = v.Notes,
                        IsAvailable = v.Status == "Available",
                        FreeAt = freeAt
                    };
                })
                .ToList();

            var model = new NewRequestDto { AvailableVehicles = vehicles };
            return View(model);
        }

        // ── GET: /EmployeeDashboard/MyRequests ───────────────────────────────
        [HttpGet]
        public ActionResult MyRequests()
        {
            string role = Session["Role"]?.ToString();
            if (role != "Employee")
                return RedirectToAction("Login", "Account");

            string empNo = Session["EmployeeNumber"]?.ToString();

            var requests = _db.VehicleRequests
                .Where(r => r.EmployeeNumber == empNo)
                .OrderByDescending(r => r.RequestedOn)
                .ToList()
                .Select(r => new MyRequestItem
                {
                    RequestId = r.RequestId,
                    VehicleName = r.VehicleName,
                    RegistrationNo = r.RegistrationNo,
                    Purpose = r.Purpose,
                    RequiredFrom = r.RequiredFrom,
                    RequiredUntil = r.RequiredUntil,
                    Status = r.Status,
                    AdminNotes = r.AdminNotes,
                    RequestedOn = r.RequestedOn
                })
                .ToList();

            var dto = new MyRequestDto { Requests = requests };
            return View(dto);
        }

        // ── POST: /EmployeeDashboard/ReturnVehicle ───────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ReturnVehicle(int requestId)
        {
            string role = Session["Role"]?.ToString();
            if (role != "Employee")
                return RedirectToAction("Login", "Account");

            string empNo = Session["EmployeeNumber"]?.ToString();

            var request = _db.VehicleRequests
                .FirstOrDefault(r => r.RequestId == requestId && r.EmployeeNumber == empNo);

            if (request != null && request.Status == "Approved")
            {
                request.Status = "Returned";

                // Set the vehicle back to Available
                var vehicle = _db.Vehicles.Find(request.VehicleId);
                if (vehicle != null)
                    vehicle.Status = "Available";

                _db.SaveChanges();
                TempData["SuccessMessage"] = "Vehicle returned successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Could not process return request.";
            }

            return RedirectToAction("MyRequests");
        }

        // ── POST: /EmployeeDashboard/SubmitRequest ───────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitRequest(VehicleRequest form)
        {
            string role = Session["Role"]?.ToString();
            if (role != "Employee")
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please fill all required fields correctly.";
                return RedirectToAction("NewRequest");
            }

            if (form.RequiredUntil <= form.RequiredFrom)
            {
                TempData["ErrorMessage"] = "'End Date' must be after 'Start Date'.";
                return RedirectToAction("NewRequest");
            }

            try
            {
                string empNo = Session["EmployeeNumber"].ToString();

                var request = new VehicleRequest
                {
                    VehicleId = form.VehicleId,
                    VehicleName = form.VehicleName,
                    RegistrationNo = form.RegistrationNo,
                    EmployeeNumber = empNo,
                    Purpose = form.Purpose,
                    RequiredFrom = form.RequiredFrom,
                    RequiredUntil = form.RequiredUntil,
                    Status = "Pending",
                    RequestedOn = DateTime.Now
                };

                _db.VehicleRequests.Add(request);
                _db.SaveChanges();

                TempData["SuccessMessage"] =
                    "Request for '" + form.VehicleName + "' (" + form.RegistrationNo + ") " +
                    "submitted successfully and is pending approval.";
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Something went wrong. Please try again.";
            }

            return RedirectToAction("NewRequest");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
