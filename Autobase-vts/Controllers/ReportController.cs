using System;
using System.Linq;
using System.Web.Mvc;
using autobase.Data;
using autobase.Models;
using autobase.Models.DTOs;

namespace autobase.Controllers
{
    public class ReportController : Controller
    {       
        private readonly AutobaseDbContext db = new AutobaseDbContext();

        // GET: Report/VehicleReport
        // preset:   "Today" | "Last5" | "Last7" | "Last30" | "Custom"
        // fromDate/toDate: only used when preset == "Custom" (or when the user picks a custom range)
        public ActionResult VehicleReport(string preset = "Last5", DateTime? fromDate = null, DateTime? toDate = null, string search = "")
        {
            string role = Session["Role"]?.ToString() ?? "";
            if (role != "SuperAdmin" && role != "Admin" && role != "HOD")
            {
                return RedirectToAction("Login", "Account");
            }

            DateTime today = DateTime.Today;
            DateTime rangeFrom;
            DateTime rangeToInclusiveEnd; // end-of-day boundary used for the query

            if (fromDate.HasValue && toDate.HasValue)
            {
                preset = "Custom";
                rangeFrom = fromDate.Value.Date;
                rangeToInclusiveEnd = toDate.Value.Date.AddDays(1).AddTicks(-1);
            }
            else
            {
                switch (preset)
                {
                    case "Today":
                        rangeFrom = today;
                        break;
                    case "Last7":
                        rangeFrom = today.AddDays(-6);
                        break;
                    case "Last30":
                        rangeFrom = today.AddDays(-29);
                        break;
                    case "Last5":
                    default:
                        preset = "Last5";
                        rangeFrom = today.AddDays(-4);
                        break;
                }
                rangeToInclusiveEnd = today.AddDays(1).AddTicks(-1);
            }

            // Only completed/approved usage counts as "used"
            var requestsQuery = db.VehicleRequests
                .Where(r => r.Status == "Approved" || r.Status == "Returned")
                .Where(r => r.RequiredFrom <= rangeToInclusiveEnd && r.RequiredUntil >= rangeFrom);

            if (!string.IsNullOrWhiteSpace(search))
            {
                string s = search.Trim().ToLower();
                requestsQuery = requestsQuery.Where(r =>
                    r.VehicleName.ToLower().Contains(s) ||
                    r.RegistrationNo.ToLower().Contains(s) ||
                    r.EmployeeNumber.ToLower().Contains(s));
            }

            var requests = requestsQuery
                .OrderByDescending(r => r.RequiredFrom)
                .ToList();            
            var employeeNumbers = requests.Select(r => r.EmployeeNumber).Distinct().ToList();
            var employees = db.Employees
                .Where(u => employeeNumbers.Contains(u.EmployeeNumber))
                .ToDictionary(u => u.EmployeeNumber, u => u);

            var items = requests.Select(r =>
            {
                Employee emp;
                employees.TryGetValue(r.EmployeeNumber, out emp);

                double usageHours = ClampHours(r.RequiredFrom, r.RequiredUntil, rangeFrom, rangeToInclusiveEnd);

                return new VehicleReportItem
                {
                    RequestId = r.RequestId,
                    VehicleId = r.VehicleId,
                    VehicleName = r.VehicleName,
                    RegistrationNo = r.RegistrationNo,
                    EmployeeName = emp != null ? emp.FullName : r.EmployeeNumber,
                    EmployeeNumber = r.EmployeeNumber,
                    Department = emp != null ? emp.Department : "-",
                    Purpose = r.Purpose,
                    ReportingPlace = r.ReportingPlace,
                    RequiredFrom = r.RequiredFrom,
                    RequiredUntil = r.RequiredUntil,
                    Status = r.Status,
                    UsageHoursInRange = usageHours
                };
            }).ToList();

            var vehicleSummaries = items
                .GroupBy(i => new { i.VehicleId, i.VehicleName, i.RegistrationNo })
                .Select(g => new VehicleUsageSummary
                {
                    VehicleId = g.Key.VehicleId,
                    VehicleName = g.Key.VehicleName,
                    RegistrationNo = g.Key.RegistrationNo,
                    TripCount = g.Count(),
                    TotalHours = Math.Round(g.Sum(x => x.UsageHoursInRange), 1),
                    LastUsedOn = g.Max(x => x.RequiredFrom)
                })
                .OrderByDescending(v => v.TripCount)
                .ThenByDescending(v => v.TotalHours)
                .ToList();

            var dto = new VehicleReportDto
            {
                Preset = preset,
                FromDate = rangeFrom,
                ToDate = rangeToInclusiveEnd.Date,
                SearchTerm = search,
                TotalTrips = items.Count,
                DistinctVehiclesUsed = items.Select(i => i.VehicleId).Distinct().Count(),
                TotalHoursUsed = Math.Round(items.Sum(i => i.UsageHoursInRange), 1),
                VehicleSummaries = vehicleSummaries,
                Items = items
            };

            return View(dto);
        }

        private double ClampHours(DateTime tripFrom, DateTime tripUntil, DateTime rangeFrom, DateTime rangeToInclusiveEnd)
        {
            var start = tripFrom < rangeFrom ? rangeFrom : tripFrom;
            var end = tripUntil > rangeToInclusiveEnd ? rangeToInclusiveEnd : tripUntil;
            var hours = (end - start).TotalHours;
            return hours > 0 ? hours : 0;
        }

        // GET: Report/VehicleHistory
        // No search  -> LIST MODE: all vehicles, most recently added first, paginated 10/page
        // With search -> DETAIL MODE: every request for the matched vehicle(s), trip-wise, paginated 10/page
        public ActionResult VehicleHistory(string search = "", string preset = "All",
            DateTime? fromDate = null, DateTime? toDate = null, int page = 1)
        {
            string role = Session["Role"]?.ToString() ?? "";
            if (role != "SuperAdmin" && role != "Admin" && role != "HOD")
            {
                return RedirectToAction("Login", "Account");
            }

            const int pageSize = 10;
            if (page < 1) page = 1;

            DateTime today = DateTime.Today;
            DateTime? rangeFrom = null;
            DateTime? rangeToInclusiveEnd = null;

            if (fromDate.HasValue && toDate.HasValue)
            {
                preset = "Custom";
                rangeFrom = fromDate.Value.Date;
                rangeToInclusiveEnd = toDate.Value.Date.AddDays(1).AddTicks(-1);
            }
            else
            {
                switch (preset)
                {
                    case "Today":
                        rangeFrom = today;
                        rangeToInclusiveEnd = today.AddDays(1).AddTicks(-1);
                        break;
                    case "Last5":
                        rangeFrom = today.AddDays(-4);
                        rangeToInclusiveEnd = today.AddDays(1).AddTicks(-1);
                        break;
                    case "Last7":
                        rangeFrom = today.AddDays(-6);
                        rangeToInclusiveEnd = today.AddDays(1).AddTicks(-1);
                        break;
                    case "Last30":
                        rangeFrom = today.AddDays(-29);
                        rangeToInclusiveEnd = today.AddDays(1).AddTicks(-1);
                        break;
                    case "Custom":                       
                        preset = "Custom";
                        rangeFrom = null;
                        rangeToInclusiveEnd = null;
                        break;
                    case "All":
                    default:
                        preset = "All";
                        rangeFrom = null;
                        rangeToInclusiveEnd = null;
                        break;
                }
            }

            var dto = new VehicleHistoryDto
            {
                SearchTerm = search,
                Preset = preset,
                FromDate = rangeFrom,
                ToDate = rangeToInclusiveEnd?.Date,
                Page = page,
                PageSize = pageSize,
                HasSearched = !string.IsNullOrWhiteSpace(search)
            };

            if (!dto.HasSearched)
            {
                // ── LIST MODE ───────────────────────────────────────────────
                var vehiclesQuery = db.Vehicles.AsQueryable();

                if (rangeFrom.HasValue && rangeToInclusiveEnd.HasValue)
                {
                    vehiclesQuery = vehiclesQuery.Where(v =>
                        v.CreatedAt >= rangeFrom.Value && v.CreatedAt <= rangeToInclusiveEnd.Value);
                }

                var allVehicles = vehiclesQuery
                    .OrderByDescending(v => v.CreatedAt)
                    .ThenByDescending(v => v.Id)
                    .ToList();

                dto.TotalItems = allVehicles.Count;

                int totalPages = dto.TotalPages;
                if (page > totalPages) page = totalPages;
                dto.Page = page;

                var pageVehicles = allVehicles
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var vehicleIds = pageVehicles.Select(v => v.Id).ToList();

                var stats = db.VehicleRequests
                    .Where(r => vehicleIds.Contains(r.VehicleId))
                    .GroupBy(r => r.VehicleId)
                    .Select(g => new
                    {
                        VehicleId = g.Key,
                        TripCount = g.Count(),
                        LastUsed = g.Max(x => (DateTime?)x.RequiredFrom)
                    })
                    .ToList()
                    .ToDictionary(x => x.VehicleId);

                dto.Vehicles = pageVehicles.Select(v =>
                {
                    int tripCount = 0;
                    DateTime? lastUsed = null;
                    if (stats.TryGetValue(v.Id, out var s))
                    {
                        tripCount = s.TripCount;
                        lastUsed = s.LastUsed;
                    }

                    return new VehicleListItem
                    {
                        VehicleId = v.Id,
                        VehicleName = v.VehicleName,
                        RegistrationNo = v.RegistrationNo,
                        VehicleType = v.VehicleType,
                        Status = v.Status,
                        AddedOn = v.CreatedAt,
                        TripCount = tripCount,
                        LastUsedOn = lastUsed
                    };
                }).ToList();

                return View(dto);
            }
            else
            {
                // ── DETAIL MODE ─────────────────────────────────────────────
                string s = search.Trim().ToLower();

                var query = db.VehicleRequests.Where(r =>
                    r.RegistrationNo.ToLower().Contains(s) ||
                    r.VehicleName.ToLower().Contains(s));

                if (rangeFrom.HasValue && rangeToInclusiveEnd.HasValue)
                {
                    query = query.Where(r => r.RequiredFrom <= rangeToInclusiveEnd.Value
                                           && r.RequiredUntil >= rangeFrom.Value);
                }

                var matched = query.OrderByDescending(r => r.RequiredFrom).ToList();

                dto.TotalItems = matched.Count;
                dto.TotalTrips = matched.Count;
                dto.DistinctVehiclesMatched = matched.Select(r => r.VehicleId).Distinct().Count();
                dto.TotalHours = Math.Round(matched.Sum(r => (r.RequiredUntil - r.RequiredFrom).TotalHours), 1);
                dto.FirstUsedOn = matched.Any() ? matched.Min(r => r.RequiredFrom) : (DateTime?)null;
                dto.LastUsedOn = matched.Any() ? matched.Max(r => r.RequiredFrom) : (DateTime?)null;

                int totalPages = dto.TotalPages;
                if (page > totalPages) page = totalPages;
                dto.Page = page;

                var pageRows = matched
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var employeeNumbers = pageRows.Select(r => r.EmployeeNumber).Distinct().ToList();
                var employees = db.Employees
                    .Where(u => employeeNumbers.Contains(u.EmployeeNumber))
                    .ToDictionary(u => u.EmployeeNumber, u => u);

                dto.Items = pageRows.Select(r =>
                {
                    Employee emp;
                    employees.TryGetValue(r.EmployeeNumber, out emp);

                    return new VehicleHistoryItem
                    {
                        RequestId = r.RequestId,
                        VehicleId = r.VehicleId,
                        VehicleName = r.VehicleName,
                        RegistrationNo = r.RegistrationNo,
                        EmployeeName = emp != null ? emp.FullName : r.EmployeeNumber,
                        EmployeeNumber = r.EmployeeNumber,
                        Department = emp != null ? emp.Department : "-",
                        Purpose = r.Purpose,
                        ReportingPlace = r.ReportingPlace,
                        RequiredFrom = r.RequiredFrom,
                        RequiredUntil = r.RequiredUntil,
                        RequestedOn = r.RequestedOn,
                        Status = r.Status,
                        AdminNotes = r.AdminNotes,
                        HodNotes = r.HodNotes,
                        HodApprovedBy = r.HodApprovedBy
                    };
                }).ToList();

                return View(dto);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}