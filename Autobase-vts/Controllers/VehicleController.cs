using System;
using System.Linq;
using System.Web.Mvc;
using autobase.Data;
using autobase.Helpers;
using autobase.Models;
using autobase.Models.DTOs;

namespace autobase.Controllers
{
    [Authorize]
    public class VehicleController : Controller
    {
        private readonly AutobaseDbContext _db = new AutobaseDbContext();

        // ── ADD VEHICLE (SuperAdmin only — Admin and HOD have no access) ───────

        // GET: /Vehicle/AddVehicle
        [RoleAuthorize("SuperAdmin")]
        [HttpGet]
        public ActionResult AddVehicle()
        {
            LoadDropdowns();
            return View("AddVehicle", new AddVehicleDto());
        }

        // POST: /Vehicle/AddVehicle
        [RoleAuthorize("SuperAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddVehicle(AddVehicleDto model)
        {
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
            return RedirectToAction("AddVehicle");
        }

        // ── EDIT LIST (SuperAdmin + Admin — HOD has no access) ──────────────────

        // GET: /Vehicle/EditVehicle (list) — paginated, 15 per page
        [RoleAuthorize("SuperAdmin", "Admin")]
        [HttpGet]
        public ActionResult EditVehicle(int page = 1)
        {
            const int pageSize = 15;
            if (page < 1) page = 1;

            var query = _db.Vehicles
                            .Where(v => v.IsActive)
                            .OrderBy(v => v.VehicleName);

            int totalCount = query.Count();
            int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            if (totalPages > 0 && page > totalPages) page = totalPages;

            var vehicles = query.Skip((page - 1) * pageSize)
                                 .Take(pageSize)
                                 .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;
            ViewBag.PageSize = pageSize;

            return View("EditVehicleList", vehicles);
        }

        // ── EDIT FORM (SuperAdmin = full edit, Admin = Status field only) ───────

        // GET: /Vehicle/EditVehicle/5
        [RoleAuthorize("SuperAdmin", "Admin")]
        [HttpGet]
        public ActionResult EditVehicleForm(int? id)
        {
            if (id == null)
                return RedirectToAction("EditVehicle");

            var vehicle = _db.Vehicles.Find(id.Value);
            if (vehicle == null || !vehicle.IsActive)
            {
                TempData["Error"] = "Vehicle not found.";
                return RedirectToAction("EditVehicle");
            }

            // SuperAdmin can edit every field. Admin can only ever change Status.
            bool isFullEdit = Session["Role"]?.ToString() == "SuperAdmin";
            ViewBag.IsFullEdit = isFullEdit;

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

        // POST: /Vehicle/EditVehicleForm
        [RoleAuthorize("SuperAdmin", "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditVehicleForm(EditVehicleDto model)
        {
            bool isFullEdit = Session["Role"]?.ToString() == "SuperAdmin";

            var vehicle = _db.Vehicles.Find(model.Id);
            if (vehicle == null || !vehicle.IsActive)
            {
                TempData["Error"] = "Vehicle not found.";
                return RedirectToAction("EditVehicle");
            }

            // ── Admin path: ONLY Status is ever written. Whatever else was
            // posted (even if someone tampered with the form/devtools) is
            // discarded server-side, not just hidden in the UI. ──────────────
            if (!isFullEdit)
            {
                ModelState.Remove("VehicleTypeId");
                ModelState.Remove("RegistrationNo");
                ModelState.Remove("YearOfManufacture");
                ModelState.Remove("Notes");

                if (!ModelState.IsValid)
                {
                    ViewBag.IsFullEdit = false;
                    ViewBag.VehicleName = vehicle.VehicleName;
                    ViewBag.VehicleType = vehicle.VehicleType;
                    LoadDropdowns(vehicle.VehicleTypeId);

                    model.VehicleTypeId = vehicle.VehicleTypeId;
                    model.RegistrationNo = vehicle.RegistrationNo;
                    model.YearOfManufacture = vehicle.YearOfManufacture;
                    model.Notes = vehicle.Notes;

                    return View("EditVehicle", model);
                }

                vehicle.Status = model.Status;
                _db.SaveChanges();

                TempData["Success"] = $"Status for \"{vehicle.VehicleName}\" ({vehicle.RegistrationNo}) updated to {vehicle.Status}.";
                return RedirectToAction("EditVehicle");
            }

            // ── SuperAdmin path: full edit (unchanged behaviour) ────────────
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
                ViewBag.IsFullEdit = true;
                LoadDropdowns(model.VehicleTypeId);
                ViewBag.VehicleName = vehicle.VehicleName;
                ViewBag.VehicleType = vehicle.VehicleType;
                return View("EditVehicle", model);
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
            return RedirectToAction("EditVehicle");
        }

        // ── DELETE (unchanged — SuperAdmin only) ─────────────────────────────

        // GET: /Vehicle/DeleteVehicle
        [RoleAuthorize("SuperAdmin")]
        [HttpGet]
        public ActionResult DeleteVehicle()
        {
            var vehicles = _db.Vehicles
                              .Where(v => v.IsActive)
                              .OrderBy(v => v.VehicleName)
                              .ToList();

            return View("DeleteVehicle", vehicles);
        }

        // POST: /Vehicle/Disable
        [RoleAuthorize("SuperAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Disable(int id)
        {
            var vehicle = _db.Vehicles.Find(id);

            if (vehicle == null || !vehicle.IsActive)
            {
                TempData["Error"] = "Vehicle not found or already disabled.";
                return RedirectToAction("DeleteVehicle");
            }

            vehicle.IsActive = false;
            _db.SaveChanges();

            TempData["Success"] = $"Vehicle \"{vehicle.VehicleName}\" ({vehicle.RegistrationNo}) has been disabled successfully.";
            return RedirectToAction("DeleteVehicle");
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
