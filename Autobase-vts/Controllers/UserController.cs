using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;
using autobase.Data;
using autobase.Models;
using autobase.Models.DTOs;

namespace autobase.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly AutobaseDbContext _db = new AutobaseDbContext();

        // ── ADD ──────────────────────────────────────────────────────────────

        [HttpGet]
        public ActionResult Add()
        {
            string role = Session["Role"]?.ToString();
            if (role != "SuperAdmin" && role != "Admin")
                return RedirectToAction("Index", "Dashboard");

            return View("AddUser", new AddUserDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(AddUserDto model)
        {
            string role = Session["Role"]?.ToString();
            if (role != "SuperAdmin" && role != "Admin")
                return RedirectToAction("Index", "Dashboard");

            if (role == "Admin" && model.Role == "SuperAdmin")
                ModelState.AddModelError("Role", "Admins cannot create SuperAdmin accounts.");

            if (!string.IsNullOrEmpty(model.EmployeeNumber))
            {
                bool exists = _db.Employees.Any(e =>
                    e.EmployeeNumber == model.EmployeeNumber.Trim() && e.IsActive);
                if (exists)
                    ModelState.AddModelError("EmployeeNumber",
                        "An employee with this employee number already exists.");
            }

            if (!ModelState.IsValid)
                return View("AddUser", model);

            var employee = new Employee
            {
                EmployeeNumber = model.EmployeeNumber.Trim().ToUpper(),
                FullName = model.FullName.Trim(),
                Email = model.Email?.Trim(),
                PasswordHash = HashPassword(model.Password),
                Role = model.Role,
                MobileNumber = model.MobileNumber?.Trim(),
                Designation = model.Designation?.Trim(),
                Department = model.Department?.Trim(),
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _db.Employees.Add(employee);
            _db.SaveChanges();

            TempData["Success"] = $"User \"{employee.FullName}\" ({employee.EmployeeNumber}) created successfully.";
            return RedirectToAction("Add");
        }

        // ── EDIT LIST ─────────────────────────────────────────────────────────

        [HttpGet]
        public ActionResult EditIndex()
        {
            string role = Session["Role"]?.ToString();
            if (role != "SuperAdmin" && role != "Admin")
                return RedirectToAction("Index", "Dashboard");

            var users = _db.Employees
                           .Where(e => e.IsActive && e.Role != "SuperAdmin")
                           .OrderBy(e => e.FullName)
                           .ToList();

            // SuperAdmin sees everyone except self; Admin sees only Employees
            if (role == "Admin")
                users = users.Where(e => e.Role == "Employee").ToList();

            return View("EditUserList", users);
        }

        // ── EDIT FORM ─────────────────────────────────────────────────────────

        [HttpGet]
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return RedirectToAction("EditIndex");

            string role = Session["Role"]?.ToString();
            if (role != "SuperAdmin" && role != "Admin")
                return RedirectToAction("Index", "Dashboard");

            var emp = _db.Employees.Find(id.Value);
            if (emp == null || !emp.IsActive)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("EditIndex");
            }

            // Admin cannot edit Admin or SuperAdmin
            if (role == "Admin" && emp.Role != "Employee")
            {
                TempData["Error"] = "You do not have permission to edit this user.";
                return RedirectToAction("EditIndex");
            }

            var dto = new EditUserDto
            {
                Id = emp.Id,
                FullName = emp.FullName,
                EmployeeNumber = emp.EmployeeNumber,
                MobileNumber = emp.MobileNumber,
                Designation = emp.Designation,
                Department = emp.Department,
                Role = emp.Role,
                Email = emp.Email
            };

            ViewBag.SessionRole = role;
            return View("EditUser", dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EditUserDto model)
        {
            string role = Session["Role"]?.ToString();
            if (role != "SuperAdmin" && role != "Admin")
                return RedirectToAction("Index", "Dashboard");

            // Duplicate employee number check (exclude self)
            if (!string.IsNullOrEmpty(model.EmployeeNumber))
            {
                bool exists = _db.Employees.Any(e =>
                    e.EmployeeNumber == model.EmployeeNumber.Trim() &&
                    e.IsActive && e.Id != model.Id);
                if (exists)
                    ModelState.AddModelError("EmployeeNumber",
                        "An employee with this employee number already exists.");
            }

            // NewPassword is optional — clear its errors if blank
            if (string.IsNullOrEmpty(model.NewPassword))
                ModelState.Remove("NewPassword");

            if (!ModelState.IsValid)
            {
                ViewBag.SessionRole = role;
                return View("EditUser", model);
            }

            var emp = _db.Employees.Find(model.Id);
            if (emp == null || !emp.IsActive)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("EditIndex");
            }

            emp.FullName = model.FullName.Trim();
            emp.EmployeeNumber = model.EmployeeNumber.Trim().ToUpper();
            emp.MobileNumber = model.MobileNumber?.Trim();
            emp.Designation = model.Designation?.Trim();
            emp.Department = model.Department?.Trim();
            emp.Role = model.Role;
            emp.Email = model.Email?.Trim();

            if (!string.IsNullOrEmpty(model.NewPassword))
                emp.PasswordHash = HashPassword(model.NewPassword);

            _db.SaveChanges();

            TempData["Success"] = $"User \"{emp.FullName}\" ({emp.EmployeeNumber}) updated successfully.";
            return RedirectToAction("EditIndex");
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private static string HashPassword(string password)
        {
            using (var sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                var sb = new StringBuilder();
                foreach (byte b in bytes) sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
