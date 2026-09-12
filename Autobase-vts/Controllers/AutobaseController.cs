using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using autobase.Data;
using autobase.Models.DTOs;
using Autobase_vts.Models.DTOs;

namespace autobase.Controllers
{
    [Authorize]
    public class AutobaseController : Controller
    {
        private readonly AutobaseDbContext _db = new AutobaseDbContext();
        private readonly QmsLookupDbContext _qmsDb = new QmsLookupDbContext();
       
        private string GetEmployeeDepartment(string employeeNumber)
        {
            if (string.IsNullOrWhiteSpace(employeeNumber)) return null;

            var emp = _db.Employees.FirstOrDefault(e => e.EmployeeNumber == employeeNumber);
            if (emp != null) return emp.Department;

            var qmsEmp = _qmsDb.EmployeeMasters.FirstOrDefault(e => e.EmployeeNo == employeeNumber);
            return qmsEmp?.Department;
        }

        private static bool SameDepartment(string a, string b) =>
            !string.IsNullOrWhiteSpace(a) && !string.IsNullOrWhiteSpace(b) &&
            a.Trim().Equals(b.Trim(), StringComparison.OrdinalIgnoreCase);

        // ── GET: /Autobase/AvailableVehicle ──────────────────────────────────
        [HttpGet]
        public ActionResult AvailableVehicle(string filter = "All", string search = "")
        {
            string role = Session["Role"]?.ToString();
            if (role == "Employee")
                return RedirectToAction("Employee", "Dashboard");

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

            var vehicles = query.OrderBy(v => v.VehicleType)
                                .ThenBy(v => v.VehicleName)
                                .ToList();

            var all = _db.Vehicles.Where(v => v.IsActive).ToList();

            var dto = new AvailableVehicleDto
            {
                TotalFleet = all.Count,
                TotalAvailable = all.Count(v => v.Status == "Available"),
                TotalInUse = all.Count(v => v.Status == "Allocated"),
                TotalMaintenance = all.Count(v => v.Status == "Maintenance"),
                ActiveFilter = filter,
                SearchTerm = search,
                Groups = vehicles
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

        // ── GET: /Autobase/AllocatedVehicle ──────────────────────────────────
        [HttpGet]
        public ActionResult AllocatedVehicle(string filter = "All", string search = "")
        {
            string role = Session["Role"]?.ToString();
            if (role != "SuperAdmin" && role != "Admin" && role != "HOD")
                return RedirectToAction("Login", "Account");

            var now = DateTime.Now;

            var approvedRequests = _db.VehicleRequests
                .Where(r => r.Status == "Approved")
                .ToList();

            var employees = _db.Employees.ToList();
            var vehicles = _db.Vehicles.ToList();

            var allItems = approvedRequests.Select(r =>
            {
                var emp = employees.FirstOrDefault(e => e.EmployeeNumber == r.EmployeeNumber);
                string name = emp != null ? emp.FullName : r.EmployeeNumber;
                string phone = emp != null ? emp.MobileNumber : "—";

                var veh = vehicles.FirstOrDefault(v => v.Id == r.VehicleId);
                string vehicleType = veh != null ? veh.VehicleType : "—";

                bool isOverdue = now > r.RequiredUntil;
                TimeSpan overdueBy = isOverdue ? (now - r.RequiredUntil) : TimeSpan.Zero;
                TimeSpan totalSpan = r.RequiredUntil - r.RequiredFrom;
                int durH = (int)totalSpan.TotalHours;
                int durM = totalSpan.Minutes;
                int totalMins = (int)totalSpan.TotalMinutes;

                int overduePercent = 0;
                if (isOverdue && totalMins > 0)
                    overduePercent = Math.Min(100, (int)((overdueBy.TotalMinutes / totalMins) * 100));

                string initials = "?";
                if (!string.IsNullOrEmpty(name))
                {
                    var parts = name.Split(' ');
                    initials = parts.Length > 1
                        ? parts[0][0].ToString().ToUpper() + parts[1][0].ToString().ToUpper()
                        : parts[0][0].ToString().ToUpper();
                }

                return new AllocatedVehicleItem
                {
                    RequestId = r.RequestId,
                    VehicleId = r.VehicleId,
                    VehicleName = r.VehicleName,
                    VehicleType = vehicleType,
                    RegistrationNo = r.RegistrationNo,
                    EmployeeName = name,
                    EmployeeNumber = r.EmployeeNumber,
                    EmployeePhone = phone,
                    Initials = initials,
                    StartTime = r.RequiredFrom,
                    DueReturn = r.RequiredUntil,
                    Purpose = r.Purpose,

                    IsOverdue = isOverdue,
                    OverdueBy = overdueBy,
                    DurationHours = durH,
                    DurationMins = durM,
                    OverduePercent = overduePercent
                };
            }).ToList();

            if (!string.IsNullOrWhiteSpace(search))
            {
                string s = search.Trim().ToLower();
                allItems = allItems.Where(x =>
                    x.VehicleName.ToLower().Contains(s) ||
                    x.RegistrationNo.ToLower().Contains(s) ||
                    x.EmployeeName.ToLower().Contains(s) ||
                    x.EmployeeNumber.ToLower().Contains(s)
                ).ToList();
            }

            if (filter == "InUse")
                allItems = allItems.Where(x => !x.IsOverdue).ToList();
            else if (filter == "Overdue")
                allItems = allItems.Where(x => x.IsOverdue).ToList();

            allItems = allItems
                .OrderByDescending(x => x.IsOverdue)
                .ThenBy(x => x.DueReturn)
                .ToList();

            var allApproved = _db.VehicleRequests.Where(r => r.Status == "Approved").ToList();

            var dto = new AllocatedVehicleDto
            {
                TotalAllocated = allApproved.Count,
                InUseCount = allApproved.Count(r => now <= r.RequiredUntil),
                OverdueCount = allApproved.Count(r => now > r.RequiredUntil),
                ActiveFilter = filter,
                SearchTerm = search,
                Items = allItems
            };

            return View(dto);
        }

        // ── POST: /Autobase/MarkReturnedFromAllocated ─────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MarkReturnedFromAllocated(int requestId)
        {
            string role = Session["Role"]?.ToString();
            if (role != "SuperAdmin" && role != "Admin" && role != "HOD")
                return RedirectToAction("Login", "Account");

            var request = _db.VehicleRequests.Find(requestId);
            if (request != null && request.Status == "Approved")
            {
                request.Status = "Returned";

                var vehicle = _db.Vehicles.Find(request.VehicleId);
                if (vehicle != null)
                    vehicle.Status = "Available";

                _db.SaveChanges();
                TempData["SuccessMessage"] = $"{request.VehicleName} marked as returned successfully.";
            }

            return RedirectToAction("AllocatedVehicle");
        }

        // ── GET: /Autobase/SeeRequests ────────────────────────────────────────
        [HttpGet]
        public ActionResult SeeRequests(string filter = "All", string search = "", DateTime? date = null)
        {
            string role = Session["Role"]?.ToString();
            if (role != "SuperAdmin" && role != "Admin" && role != "HOD")
                return RedirectToAction("Login", "Account");

            DateTime selectedDate = (date ?? DateTime.Today).Date;
            DateTime nextDay = selectedDate.AddDays(1);

            var dateRequests = _db.VehicleRequests
                .Where(r => r.RequestedOn >= selectedDate && r.RequestedOn < nextDay)
                .ToList();
            
            var autobaseEmployees = _db.Employees.ToList();
            var qmsEmployees = _qmsDb.EmployeeMasters.ToList();

            Func<string, autobase.Models.Employee> findAutobase = empNo =>
                autobaseEmployees.FirstOrDefault(e => e.EmployeeNumber == empNo);
            Func<string, QmsEmployeeMasterLite> findQms = empNo =>
                qmsEmployees.FirstOrDefault(e => e.EmployeeNo == empNo);
            
            if (role == "HOD")
            {
                var hodEmp = findAutobase(Session["EmployeeNumber"]?.ToString());
                string hodDept = hodEmp?.Department;   // HODs are always created via Autobase's own Employees table

                dateRequests = dateRequests
                    .Where(r =>
                    {
                        var e = findAutobase(r.EmployeeNumber);
                        string reqDept = e != null ? e.Department : findQms(r.EmployeeNumber)?.Department;
                        return SameDepartment(reqDept, hodDept);
                    })
                    .ToList();
            }

            var requestsQuery = dateRequests.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                string s = search.Trim().ToLower();
                requestsQuery = requestsQuery.Where(r =>
                    r.VehicleName.ToLower().Contains(s) ||
                    r.RegistrationNo.ToLower().Contains(s) ||
                    r.EmployeeNumber.ToLower().Contains(s) ||
                    r.Purpose.ToLower().Contains(s));
            }

            if (filter == "Pending")
                requestsQuery = requestsQuery.Where(r => r.Status == "Pending");
            else if (filter == "HODApproved")
                requestsQuery = requestsQuery.Where(r => r.Status == "HODApproved");
            else if (filter == "Approved")
                requestsQuery = requestsQuery.Where(r => r.Status == "Approved");
            else if (filter == "Rejected")
                requestsQuery = requestsQuery.Where(r => r.Status == "Rejected");
            else if (filter == "Completed")
                requestsQuery = requestsQuery.Where(r => r.Status == "Returned");

            var filtered = requestsQuery.OrderByDescending(r => r.RequestedOn).ToList();
            
            var items = filtered.Select(r =>
            {
                var emp = findAutobase(r.EmployeeNumber);
                var qmsEmp = emp == null ? findQms(r.EmployeeNumber) : null;

                return new SeeRequestItem
                {
                    RequestId = r.RequestId,
                    EmployeeName = emp != null ? emp.FullName : (qmsEmp != null ? qmsEmp.EmployeeName : r.EmployeeNumber),
                    EmployeeNumber = r.EmployeeNumber,
                    EmployeePhone = emp != null ? emp.MobileNumber : "—",
                    Department = emp != null ? emp.Department : (qmsEmp != null ? qmsEmp.Department : "—"),
                    Designation = emp != null ? emp.Designation : (qmsEmp != null ? qmsEmp.Designation : "—"),
                    VehicleName = r.VehicleName,
                    RegistrationNo = r.RegistrationNo,
                    Purpose = r.Purpose,
                    ReportingPlace = r.ReportingPlace,
                    RequiredFrom = r.RequiredFrom,
                    RequiredUntil = r.RequiredUntil,
                    Status = r.Status,
                    AdminNotes = r.AdminNotes,
                    RequestedOn = r.RequestedOn,
                    HodNotes = r.HodNotes,
                    HodApprovedBy = r.HodApprovedBy,
                    HodApprovedOn = r.HodApprovedOn,
                    FinalApprovedBy = r.FinalApprovedBy,
                    FinalApprovedOn = r.FinalApprovedOn
                };
            }).ToList();

            var dto = new SeeRequestDto
            {
                TotalCount = dateRequests.Count,
                PendingCount = dateRequests.Count(r => r.Status == "Pending"),
                HodApprovedCount = dateRequests.Count(r => r.Status == "HODApproved"),
                ApprovedCount = dateRequests.Count(r => r.Status == "Approved"),
                RejectedCount = dateRequests.Count(r => r.Status == "Rejected"),
                CompletedCount = dateRequests.Count(r => r.Status == "Returned"),
                ActiveFilter = filter,
                SearchTerm = search,
                SelectedDate = selectedDate,
                Requests = items
            };

            return View(dto);
        }

        // ── POST: /Autobase/ApproveRequest ────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ApproveRequest(int requestId, string adminNotes)
        {
            string role = Session["Role"]?.ToString();
            if (role != "SuperAdmin" && role != "Admin" && role != "HOD")
                return RedirectToAction("Login", "Account");

            var request = _db.VehicleRequests.Find(requestId);
            if (request == null)
                return RedirectToAction("SeeRequests");

            string approverName = Session["FullName"]?.ToString() ?? role;

            if (role == "HOD")
            {
                if (request.Status != "Pending")
                {
                    TempData["ErrorMessage"] = "This request is not awaiting HOD approval.";
                    return RedirectToAction("SeeRequests");
                }

                // ── FIXED: extract session value first, then query ──
                string currentEmpNumber = Session["EmployeeNumber"]?.ToString();
                string hodDept = _db.Employees
                    .FirstOrDefault(e => e.EmployeeNumber == currentEmpNumber)
                    ?.Department;
                string requesterDept = GetEmployeeDepartment(request.EmployeeNumber);

                if (!SameDepartment(requesterDept, hodDept))
                {
                    TempData["ErrorMessage"] = "You can only approve requests from your own department.";
                    return RedirectToAction("SeeRequests");
                }

                request.Status = "HODApproved";
                request.HodNotes = adminNotes;
                request.HodApprovedBy = approverName;
                request.HodApprovedOn = DateTime.Now;

                _db.SaveChanges();
                TempData["SuccessMessage"] = "Request approved. Awaiting final approval from Admin.";
            }
            else if (role == "SuperAdmin")
            {
                if (request.Status != "Pending" && request.Status != "HODApproved")
                {
                    TempData["ErrorMessage"] = "This request cannot be approved from its current status.";
                    return RedirectToAction("SeeRequests");
                }

                if (request.Status == "Pending")
                {
                    request.HodApprovedBy = $"{approverName} (SuperAdmin — HOD step skipped)";
                    request.HodApprovedOn = DateTime.Now;
                }

                request.Status = "Approved";
                request.AdminNotes = adminNotes;
                request.FinalApprovedBy = approverName;
                request.FinalApprovedOn = DateTime.Now;

                var vehicle = _db.Vehicles.Find(request.VehicleId);
                if (vehicle != null)
                    vehicle.Status = "Allocated";

                _db.SaveChanges();
                TempData["SuccessMessage"] = "Request approved successfully.";
            }
            else // Admin — final approval ONLY, cannot skip HOD
            {
                if (request.Status != "HODApproved")
                {
                    TempData["ErrorMessage"] = "This request must be approved by HOD before Admin can give final approval.";
                    return RedirectToAction("SeeRequests");
                }

                request.Status = "Approved";
                request.AdminNotes = adminNotes;
                request.FinalApprovedBy = approverName;
                request.FinalApprovedOn = DateTime.Now;

                var vehicle = _db.Vehicles.Find(request.VehicleId);
                if (vehicle != null)
                    vehicle.Status = "Allocated";

                _db.SaveChanges();
                TempData["SuccessMessage"] = "Request approved successfully.";
            }

            return RedirectToAction("SeeRequests");
        }

        // ── POST: /Autobase/RejectRequest ─────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RejectRequest(int requestId, string adminNotes)
        {
            string role = Session["Role"]?.ToString();
            if (role != "SuperAdmin" && role != "Admin" && role != "HOD")
                return RedirectToAction("Login", "Account");

            var request = _db.VehicleRequests.Find(requestId);
            if (request == null)
                return RedirectToAction("SeeRequests");

            if (role == "HOD")
            {
                if (request.Status != "Pending")
                {
                    TempData["ErrorMessage"] = "This request is not awaiting HOD approval.";
                    return RedirectToAction("SeeRequests");
                }
                
                string currentEmpNumber = Session["EmployeeNumber"]?.ToString();
                string hodDept = _db.Employees
                    .FirstOrDefault(e => e.EmployeeNumber == currentEmpNumber)
                    ?.Department;
                string requesterDept = GetEmployeeDepartment(request.EmployeeNumber);

                if (!SameDepartment(requesterDept, hodDept))
                {
                    TempData["ErrorMessage"] = "You can only reject requests from your own department.";
                    return RedirectToAction("SeeRequests");
                }
            }

            if (request.Status == "Pending" || request.Status == "HODApproved")
            {
                request.Status = "Rejected";
                request.AdminNotes = adminNotes;
                _db.SaveChanges();
                TempData["SuccessMessage"] = "Request rejected.";
            }

            return RedirectToAction("SeeRequests");
        }

        // ── POST: /Autobase/MarkReturned ──────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MarkReturned(int requestId)
        {
            string role = Session["Role"]?.ToString();
            if (role != "SuperAdmin" && role != "Admin" && role != "HOD")
                return RedirectToAction("Login", "Account");

            var request = _db.VehicleRequests.Find(requestId);
            if (request != null && request.Status == "Approved")
            {
                request.Status = "Returned";

                var vehicle = _db.Vehicles.Find(request.VehicleId);
                if (vehicle != null)
                    vehicle.Status = "Available";

                _db.SaveChanges();
                TempData["SuccessMessage"] = "Vehicle marked as returned.";
            }

            return RedirectToAction("SeeRequests");
        }

        // ── GET: /Autobase/PrintRequest ───────────────────────────────────────
        [HttpGet]
        public ActionResult PrintRequest(int id)
        {
            string role = Session["Role"]?.ToString();
            if (role != "SuperAdmin" && role != "Admin" && role != "HOD")
                return RedirectToAction("Login", "Account");

            var req = _db.VehicleRequests.Find(id);
            if (req == null)
                return HttpNotFound();

            var emp = _db.Employees.FirstOrDefault(e => e.EmployeeNumber == req.EmployeeNumber);
            var qmsEmp = emp == null
                ? _qmsDb.EmployeeMasters.FirstOrDefault(e => e.EmployeeNo == req.EmployeeNumber)
                : null;

            double durationHours = (req.RequiredUntil - req.RequiredFrom).TotalHours;

            var dto = new PrintRequestDto
            {
                RequestId = req.RequestId,
                EmployeeName = emp != null ? emp.FullName : (qmsEmp != null ? qmsEmp.EmployeeName : req.EmployeeNumber),
                EmployeeNumber = req.EmployeeNumber ?? string.Empty,
                EmployeePhone = emp != null ? emp.MobileNumber : "—",
                Designation = emp != null ? emp.Designation : (qmsEmp != null ? qmsEmp.Designation : "—"),
                Department = emp != null ? emp.Department : (qmsEmp != null ? qmsEmp.Department : "—"),
                VehicleName = req.VehicleName ?? string.Empty,
                RegistrationNo = req.RegistrationNo ?? string.Empty,
                RequiredFrom = req.RequiredFrom,
                RequiredUntil = req.RequiredUntil,
                DurationHours = durationHours,
                Purpose = req.Purpose ?? string.Empty,
                AdminNotes = req.AdminNotes ?? string.Empty,
                RequestedOn = req.RequestedOn,
                Status = req.Status ?? string.Empty,
            };

            return View(dto);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
                _qmsDb.Dispose(); 
            }
            base.Dispose(disposing);
        }
    }
}