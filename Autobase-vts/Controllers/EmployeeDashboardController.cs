using System;
using System.Linq;
using System.Web.Mvc;
using autobase.Data;
using autobase.Models;
using autobase.Models.DTOs;
using Autobase_vts.Models.DTOs;

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

            string empNo = Session["EmployeeNumber"]?.ToString();
            string empName = Session["FullName"]?.ToString() ?? "Employee";
            var now = DateTime.Now;

            // ── All requests for this employee ──────────────────────────────────
            var allRequests = _db.VehicleRequests
                .Where(r => r.EmployeeNumber == empNo)
                .OrderByDescending(r => r.RequestedOn)
                .ToList();

            // ── Recent 5 (for the table) ────────────────────────────────────────
            var recentItems = allRequests
                .Take(5)
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

            // ── Currently approved (allocated) vehicles ─────────────────────────
            var approvedRequests = allRequests
                .Where(r => r.Status == "Approved")
                .ToList();

            var allocatedItems = approvedRequests.Select(r =>
            {
                var duration = r.RequiredUntil - r.RequiredFrom;
                bool isOverdue = now > r.RequiredUntil;
                var overdueBy = isOverdue ? now - r.RequiredUntil : TimeSpan.Zero;
                int overduePercent = 0;
                if (isOverdue && duration.TotalMinutes > 0)
                    overduePercent = (int)Math.Min(100, (overdueBy.TotalMinutes / duration.TotalMinutes) * 100);

                return new AllocatedVehicleItem
                {
                    RequestId = r.RequestId,
                    VehicleId = r.VehicleId,
                    VehicleName = r.VehicleName,
                    VehicleType = "",
                    RegistrationNo = r.RegistrationNo,
                    EmployeeName = empName,
                    EmployeeNumber = empNo,
                    EmployeePhone = "",
                    Initials = empName.Substring(0, 1).ToUpper(),
                    StartTime = r.RequiredFrom,
                    DueReturn = r.RequiredUntil,
                    Purpose = r.Purpose,
                    IsOverdue = isOverdue,
                    OverdueBy = overdueBy,
                    DurationHours = (int)duration.TotalHours,
                    DurationMins = duration.Minutes,
                    OverduePercent = overduePercent
                };
            }).ToList();

            var dto = new EmployeeDashboardDto
            {
                TotalRequests = allRequests.Count,
                ApprovedCount = allRequests.Count(r => r.Status == "Approved"),
                PendingCount = allRequests.Count(r => r.Status == "Pending"),
                RejectedCount = allRequests.Count(r => r.Status == "Rejected"),
                AllocatedCount = approvedRequests.Count,
                RecentRequests = recentItems,
                AllocatedVehicles = allocatedItems
            };

            return View("~/Views/Dashboard/EmployeeDashboard.cshtml", dto);
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
        public ActionResult NewRequest(int page = 1, string filter = "All")
        {
            string role = Session["Role"]?.ToString();
            if (role != "Employee")
                return RedirectToAction("Login", "Account");

            const int pageSize = 10;
            if (page < 1) page = 1;

            // ── Full fleet, unfiltered — used only for the summary chip counts ──
            var fullFleet = _db.Vehicles.Where(v => v.IsActive).ToList();
            int totalAvailable = fullFleet.Count(v => v.Status == "Available");
            int totalInUse = fullFleet.Count(v => v.Status == "Allocated");
            int totalMaintenance = fullFleet.Count(v => v.Status == "Maintenance");

            // ── Apply the status filter BEFORE paginating ──
            var query = _db.Vehicles.Where(v => v.IsActive);

            if (filter == "Available")
                query = query.Where(v => v.Status == "Available");
            else if (filter == "InUse")
                query = query.Where(v => v.Status == "Allocated");
            else if (filter == "Maintenance")
                query = query.Where(v => v.Status == "Maintenance");
            // "All" → no extra filter

            var filteredVehicles = query
                .OrderBy(v => v.VehicleType)
                .ThenBy(v => v.VehicleName)
                .ToList()
                .Select(v =>
                {
                    DateTime? freeAt = null;

                    if (v.Status == "Allocated")
                    {
                        var activeRequest = _db.VehicleRequests
                            .Where(r => r.VehicleId == v.Id && r.Status == "Approved")
                            .OrderByDescending(r => r.RequiredUntil)
                            .FirstOrDefault();

                        freeAt = activeRequest?.RequiredUntil;
                    }

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
                        Status = v.Status,
                        IsAvailable = v.Status == "Available",
                        FreeAt = freeAt
                    };
                })
                .ToList();

            // ── Paginate the FILTERED list, not the full fleet ──
            int totalCount = filteredVehicles.Count;
            int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            if (totalPages < 1) totalPages = 1;
            if (page > totalPages) page = totalPages;

            var pageItems = filteredVehicles
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var model = new NewRequestDto
            {
                AvailableVehicles = pageItems,
                CurrentPage = page,
                TotalPages = totalPages,
                PageSize = pageSize,
                TotalVehicleCount = totalCount,
                TotalAvailable = totalAvailable,
                TotalInUse = totalInUse,
                TotalMaintenance = totalMaintenance,
                ActiveFilter = filter
            };

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
        [HttpGet]
        public ActionResult AllocatedVehicle()
        {
            string role = Session["Role"]?.ToString();
            if (role != "Employee")
                return RedirectToAction("Login", "Account");

            string empNo = Session["EmployeeNumber"]?.ToString();

            var now = DateTime.Now;

            // Only grab this employee Approved requests
            var approvedRequests = _db.VehicleRequests
                .Where(r => r.EmployeeNumber == empNo && r.Status == "Approved")
                .OrderByDescending(r => r.RequiredFrom)
                .ToList();

            var items = approvedRequests.Select(r =>
            {
                var duration = r.RequiredUntil - r.RequiredFrom;
                bool isOverdue = now > r.RequiredUntil;
                var overdueBy = isOverdue ? now - r.RequiredUntil : TimeSpan.Zero;

                // Overdure progress: 0-100 capped, based on how far past due-time we are
                // relative to total duration (so a short overdue on a long booking feels right)
                int overduePercent = 0;
                if (isOverdue && duration.TotalMinutes > 0)
                {
                    overduePercent = (int)Math.Min(100,
                        (overdueBy.TotalMinutes / duration.TotalMinutes) * 100);
                }

                string empName = Session["FullName"]?.ToString() ?? "Employee";

                return new AllocatedVehicleItem
                {
                    RequestId = r.RequestId,
                    VehicleId = r.VehicleId,
                    VehicleName = r.VehicleName,
                    VehicleType = "", 
                    RegistrationNo = r.RegistrationNo,

                    
                    EmployeeName = empName,
                    EmployeeNumber = empNo,
                    EmployeePhone = "", 
                    Initials = string.IsNullOrEmpty(empName) ? "E"
                                     : empName.Substring(0, 1).ToUpper(),

                    // Request timing
                    StartTime = r.RequiredFrom,
                    DueReturn = r.RequiredUntil,
                    Purpose = r.Purpose,

                    // Computed
                    IsOverdue = isOverdue,
                    OverdueBy = overdueBy,
                    DurationHours = (int)duration.TotalHours,
                    DurationMins = duration.Minutes,
                    OverduePercent = overduePercent
                };
            }).ToList();

            var dto = new AllocatedVehicleDto
            {
                TotalAllocated = items.Count,
                InUseCount = items.Count(x => !x.IsOverdue),
                OverdueCount = items.Count(x => x.IsOverdue),
                ActiveFilter = "All",
                SearchTerm = "",
                Items = items
            };

            return View(dto);    // renders Views/EmployeeDashboard/AllocatedVehicle.cshtml
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
