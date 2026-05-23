// Controllers/AutobaseController.cs
using System.Linq;
using System.Web.Mvc;
using autobase.Data;
using autobase.Models.DTOs;

namespace autobase.Controllers
{
    [Authorize]
    public class AutobaseController : Controller
    {
        private readonly AutobaseDbContext _db = new AutobaseDbContext();

        // GET: /Autobase/AvailableVehicle
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

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}