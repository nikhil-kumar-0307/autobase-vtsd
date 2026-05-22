using System;
using System.Linq;
using System.Web.Mvc;
using autobase.Data;
using autobase.Models;
using autobase.Models.DTOs;

namespace autobase.Controllers
{
    [Authorize]
    public class VehicleController : Controller
    {
        private readonly AutobaseDbContext _db = new AutobaseDbContext();

        // GET: /Vehicle/Add
        [HttpGet]
        public ActionResult Add()
        {
            string role = Session["Role"]?.ToString();
            if (role == "Employee")
                return RedirectToAction("Employee", "Dashboard");

            LoadDropdowns();
            return View("AddVehicle", new AddVehicleDto());
        }

        // POST: /Vehicle/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(AddVehicleDto model)
        {
            string role = Session["Role"]?.ToString();
            if (role == "Employee")
                return RedirectToAction("Employee", "Dashboard");

            // Duplicate registration check
            if (!string.IsNullOrEmpty(model.RegistrationNo))
            {
                bool regExists = _db.Vehicles.Any(v =>
                    v.RegistrationNo == model.RegistrationNo.Trim() && v.IsActive);
                if (regExists)
                    ModelState.AddModelError("RegistrationNo",
                        "A vehicle with this registration number already exists.");
            }

            if (!ModelState.IsValid)
            {
                LoadDropdowns(model.VehicleTypeId);
                return View("AddVehicle" , model);
            }

            var vehicleType = _db.VehicleTypes.Find(model.VehicleTypeId);

            int empId = 0;
            int.TryParse(Session["EmployeeId"]?.ToString(), out empId);

            var vehicle = new Vehicle
            {
                VehicleTypeId = model.VehicleTypeId,
                VehicleName = vehicleType.VehicleName,
                VehicleType = vehicleType.VehicleType,
                RegistrationNo = model.RegistrationNo.Trim().ToUpper(),
                YearOfManufacture = model.YearOfManufacture.Value,
                Status = model.Status,
                Notes = model.Notes?.Trim(),
                IsActive = true,
                CreatedAt = DateTime.Now,
                CreatedBy = empId
            };

            _db.Vehicles.Add(vehicle);
            _db.SaveChanges();

            TempData["Success"] = $"Vehicle \"{vehicle.VehicleName}\" ({vehicle.RegistrationNo}) registered successfully.";
            return RedirectToAction("Add");
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private void LoadDropdowns(int selectedTypeId = 0)
        {
            var types = _db.VehicleTypes
                           .Where(v => v.IsActive)
                           .OrderBy(v => v.VehicleName)
                           .ToList();

            ViewBag.VehicleTypes = types;
            ViewBag.SelectedTypeId = selectedTypeId;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
