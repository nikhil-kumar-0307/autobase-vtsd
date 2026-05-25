// Controllers/AccountController.cs
using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using autobase.Data;
using autobase.Helpers;
using autobase.Models.DTOs;

namespace autobase.Controllers
{
    public class AccountController : Controller
    {
        private readonly AutobaseDbContext _db = new AutobaseDbContext();

        // GET: /Account/Login  (default route lands here)
        [HttpGet]
        public ActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                // Restore session if empty (RememberMe case)
                if (Session["Role"] == null)
                {
                    var emp = _db.Employees
                        .FirstOrDefault(e => e.EmployeeNumber == User.Identity.Name && e.IsActive);

                    if (emp != null)
                    {
                        Session["Role"] = emp.Role;
                        Session["FullName"] = emp.FullName;
                        Session["EmployeeNumber"] = emp.EmployeeNumber;
                        Session["EmployeeId"] = emp.Id;
                    }
                }
                return RedirectToRole();
            }
            return View(new LoginDto());
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginDto model, string returnUrl)
        {
            if (!ModelState.IsValid)
                return View(model);

            var employee = _db.Employees
                .FirstOrDefault(e => e.EmployeeNumber == model.EmployeeNumber && e.IsActive);

            if (employee == null || !PasswordHelper.VerifyPassword(model.Password, employee.PasswordHash))
            {
                ModelState.AddModelError("", "Invalid employee number or password.");
                return View(model);
            }

            // Update last login
            employee.LastLoginAt = DateTime.Now;
            _db.SaveChanges();

            // Issue forms auth ticket
            FormsAuthentication.SetAuthCookie(employee.EmployeeNumber, model.RememberMe);

            // Store role info in session for sidebar / topbar
            Session["Role"] = employee.Role;
            Session["FullName"] = employee.FullName;
            Session["EmployeeNumber"] = employee.EmployeeNumber;
            Session["EmployeeId"] = employee.Id;

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToRole(employee.Role);
        }

        // GET: /Account/Logout
        [HttpGet]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Login");
        }

        // ── Helpers ──────────────────────────────────────────────────────────        

        private ActionResult RedirectToRole(string role = null)
        {
            role = role ?? Session["Role"]?.ToString() ?? "Employee";
            if (role == "SuperAdmin" || role == "Admin")
                return RedirectToAction("Index", "Dashboard");

            return RedirectToAction("EmployeeDashboard", "Dashboard");
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
