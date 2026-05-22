// Controllers/VehicleTypeController.cs
using System;
using System.Linq;
using System.Web.Mvc;
using autobase.Data;
using autobase.Models;
using autobase.Models.DTOs;

namespace autobase.Controllers
{
    [Authorize]
    public class VehicleTypeController : Controller
    {
        private readonly AutobaseDbContext _db = new AutobaseDbContext();

        // GET: /VehicleType/Create
        [HttpGet]
        public ActionResult Create()
        {
            string role = Session["Role"]?.ToString();
            if (role == "Employee")
                return RedirectToAction("Employee", "Dashboard");

            // Pass existing vehicles list to the view
            ViewBag.Vehicles = _db.VehicleTypes
                                   .Where(v => v.IsActive)
                                   .OrderByDescending(v => v.CreatedAt)
                                   .ToList();

            return View(new VehicleTypesDto());
        }

        // POST: /VehicleType/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(VehicleTypesDto model)
        {
            string role = Session["Role"]?.ToString();
            if (role == "Employee")
                return RedirectToAction("Employee", "Dashboard");

            if (!ModelState.IsValid)
            {
                ViewBag.Vehicles = _db.VehicleTypes
                                       .Where(v => v.IsActive)
                                       .OrderByDescending(v => v.CreatedAt)
                                       .ToList();
                return View(model);
            }

            // Check for duplicate (same name + type)
            bool exists = _db.VehicleTypes.Any(v =>
                v.VehicleName == model.VehicleName &&
                v.VehicleType == model.VehicleType &&
                v.IsActive);

            if (exists)
            {
                ModelState.AddModelError("", "A vehicle with this name and type already exists.");
                ViewBag.Vehicles = _db.VehicleTypes
                                       .Where(v => v.IsActive)
                                       .OrderByDescending(v => v.CreatedAt)
                                       .ToList();
                return View(model);
            }

            int empId = 0;
            int.TryParse(Session["EmployeeId"]?.ToString(), out empId);

            var vehicle = new VehicleTypes
            {
                VehicleName = model.VehicleName.Trim(),
                VehicleType = model.VehicleType.Trim(),
                IsActive = true,
                CreatedAt = DateTime.Now,
                CreatedBy = empId
            };

            _db.VehicleTypes.Add(vehicle);
            _db.SaveChanges();

            TempData["Success"] = $"Vehicle \"{vehicle.VehicleName}\" ({vehicle.VehicleType}) added successfully.";
            return RedirectToAction("Create");
        }

        // POST: /VehicleType/Delete/5  (soft-delete)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var vehicle = _db.VehicleTypes.Find(id);
            if (vehicle != null)
            {
                vehicle.IsActive = false;
                _db.SaveChanges();
                TempData["Success"] = "Vehicle removed successfully.";
            }
            return RedirectToAction("Create");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}