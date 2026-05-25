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

        // ── ADD ──────────────────────────────────────────────────────────────

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
                return View("AddVehicle", model);
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

        // ── EDIT LIST ────────────────────────────────────────────────────────

        // GET: /Vehicle/EditIndex
        [HttpGet]
        public ActionResult EditIndex()
        {
            string role = Session["Role"]?.ToString();
            if (role == "Employee")
                return RedirectToAction("Employee", "Dashboard");

            var vehicles = _db.Vehicles
                              .Where(v => v.IsActive)
                              .OrderBy(v => v.VehicleName)
                              .ToList();

            return View("EditVehicleList", vehicles);
        }

        // ── EDIT FORM ────────────────────────────────────────────────────────

        // GET: /Vehicle/Edit/5
        [HttpGet]
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return RedirectToAction("EditIndex");

            string role = Session["Role"]?.ToString();
            if (role == "Employee")
                return RedirectToAction("Employee", "Dashboard");

            var vehicle = _db.Vehicles.Find(id.Value);
            if (vehicle == null || !vehicle.IsActive)
            {
                TempData["Error"] = "Vehicle not found.";
                return RedirectToAction("EditIndex");
            }

            LoadDropdowns(vehicle.VehicleTypeId);

            var dto = new EditVehicleDto
            {
                Id = vehicle.Id,
                VehicleTypeId = vehicle.VehicleTypeId,
                RegistrationNo = vehicle.RegistrationNo,
                YearOfManufacture = vehicle.YearOfManufacture,
                Status = vehicle.Status,
                Notes = vehicle.Notes
            };

            ViewBag.VehicleName = vehicle.VehicleName;
            ViewBag.VehicleType = vehicle.VehicleType;

            return View("EditVehicle", dto);
        }

        // POST: /Vehicle/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EditVehicleDto model)
        {
            string role = Session["Role"]?.ToString();
            if (role == "Employee")
                return RedirectToAction("Employee", "Dashboard");

            // Duplicate registration check — exclude current vehicle
            if (!string.IsNullOrEmpty(model.RegistrationNo))
            {
                bool regExists = _db.Vehicles.Any(v =>
                    v.RegistrationNo == model.RegistrationNo.Trim() &&
                    v.IsActive &&
                    v.Id != model.Id);

                if (regExists)
                    ModelState.AddModelError("RegistrationNo",
                        "A vehicle with this registration number already exists.");
            }

            if (!ModelState.IsValid)
            {
                LoadDropdowns(model.VehicleTypeId);

                var existing = _db.Vehicles.Find(model.Id);
                ViewBag.VehicleName = existing?.VehicleName;
                ViewBag.VehicleType = existing?.VehicleType;

                return View("EditVehicle", model);
            }

            var vehicle = _db.Vehicles.Find(model.Id);
            if (vehicle == null || !vehicle.IsActive)
            {
                TempData["Error"] = "Vehicle not found.";
                return RedirectToAction("EditIndex");
            }

            var vehicleType = _db.VehicleTypes.Find(model.VehicleTypeId);

            vehicle.VehicleTypeId = model.VehicleTypeId;
            vehicle.VehicleName = vehicleType.VehicleName;
            vehicle.VehicleType = vehicleType.VehicleType;
            vehicle.RegistrationNo = model.RegistrationNo.Trim().ToUpper();
            vehicle.YearOfManufacture = model.YearOfManufacture.Value;
            vehicle.Status = model.Status;
            vehicle.Notes = model.Notes?.Trim();

            _db.SaveChanges();

            TempData["Success"] = $"Vehicle \"{vehicle.VehicleName}\" ({vehicle.RegistrationNo}) updated successfully.";
            return RedirectToAction("EditIndex");
        }

        // GET: /Vehicle/Delete
        [HttpGet]
        public ActionResult Delete()
        {
            string role = Session["Role"]?.ToString();
            if (role == "Employee")
                return RedirectToAction("Employee", "Dashboard");

            var vehicles = _db.Vehicles
                              .Where(v => v.IsActive)
                              .OrderBy(v => v.VehicleName)
                              .ToList();

            return View("DeleteVehicle", vehicles);   
        } 

        // POST: /Vehicle/Disable
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Disable(int id)
        {
            string role = Session["Role"]?.ToString();
            if (role == "Employee")
                return RedirectToAction("Employee", "Dashboard");

            var vehicle = _db.Vehicles.Find(id);

            if (vehicle == null || !vehicle.IsActive)
            {
                TempData["Error"] = "Vehicle not found or already disabled.";
                return RedirectToAction("Delete");
            }

            vehicle.IsActive = false; // soft-delete — data stays in DB
            _db.SaveChanges();

            TempData["Success"] = $"Vehicle \"{vehicle.VehicleName}\" ({vehicle.RegistrationNo}) has been disabled successfully.";
            return RedirectToAction("Delete");
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