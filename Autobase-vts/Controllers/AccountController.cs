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

        // GET: /Account/Login
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
                    else
                    {
                        // The remembered cookie might belong to a QMS-sourced login instead
                        using (var qmsDb = new QmsLookupDbContext())
                        {
                            var qmsEmp = qmsDb.EmployeeMasters
                                .FirstOrDefault(e => e.EmployeeNo == User.Identity.Name);

                            if (qmsEmp != null)
                            {
                                Session["Role"] = "Employee";
                                Session["FullName"] = qmsEmp.EmployeeName;
                                Session["EmployeeNumber"] = qmsEmp.EmployeeNo;
                                Session["EmployeeId"] = 0; // see note below
                                Session["FromQMS"] = true;
                            }
                            else
                            {
                                // Cookie no longer matches either source — force a fresh login
                                FormsAuthentication.SignOut();
                                Session.Clear();
                                return View(new LoginDto());
                            }
                        }
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

            // 1) Try Autobase's own Employees table first — this is the only
            //    source that can produce SuperAdmin/Admin/HOD roles.
            var employee = _db.Employees
                .FirstOrDefault(e => e.EmployeeNumber == model.EmployeeNumber && e.IsActive);

            if (employee != null && PasswordHelper.VerifyPassword(model.Password, employee.PasswordHash))
            {
                employee.LastLoginAt = DateTime.Now;
                _db.SaveChanges();

                FormsAuthentication.SetAuthCookie(employee.EmployeeNumber, model.RememberMe);
                Session["Role"] = employee.Role;
                Session["FullName"] = employee.FullName;
                Session["EmployeeNumber"] = employee.EmployeeNumber;
                Session["EmployeeId"] = employee.Id;

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);
                return RedirectToRole(employee.Role);
            }

            // 2) Fall back to QMS's EmployeeMaster table. There's no Role column
            //    there, so anyone who logs in this way is always a plain "Employee".
            using (var qmsDb = new QmsLookupDbContext())
            {
                var qmsEmployee = qmsDb.EmployeeMasters
                    .FirstOrDefault(e => e.EmployeeNo == model.EmployeeNumber);

                // QMS stores the password as plain text (see EmployeeMasterController),
                // so this is a direct string comparison — NOT run through PasswordHelper.
                if (qmsEmployee != null && qmsEmployee.Password == model.Password)
                {
                    FormsAuthentication.SetAuthCookie(qmsEmployee.EmployeeNo, model.RememberMe);
                    Session["Role"] = "Employee";
                    Session["FullName"] = qmsEmployee.EmployeeName;
                    Session["EmployeeNumber"] = qmsEmployee.EmployeeNo;
                    Session["EmployeeId"] = 0; // see note below — not an Autobase Employees.Id
                    Session["FromQMS"] = true;

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        return Redirect(returnUrl);
                    return RedirectToAction("EmployeeDashboard", "Dashboard");
                }
            }

            ModelState.AddModelError("", "Invalid employee number or password.");
            return View(model);
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
            if (role == "SuperAdmin" || role == "Admin" || role == "HOD")
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