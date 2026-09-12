using System;
using System.Linq;
using System.Web.Mvc;
using autobase.Data;
using autobase.Helpers;

namespace autobase.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly AutobaseDbContext _db = new AutobaseDbContext();
        private readonly QmsLookupDbContext _qmsDb = new QmsLookupDbContext();

        // ── GET: /Profile/Details ──────────────────────────────────────────
        [HttpGet]
        public JsonResult Details()
        {
            string empNo = Session["EmployeeNumber"]?.ToString();
            if (string.IsNullOrEmpty(empNo))
                return Json(new { success = false, message = "Session expired. Please log in again." }, JsonRequestBehavior.AllowGet);

            var emp = _db.Employees.FirstOrDefault(e => e.EmployeeNumber == empNo);
            if (emp != null)
            {
                return Json(new
                {
                    success = true,
                    fullName = emp.FullName,
                    employeeNumber = emp.EmployeeNumber,
                    department = emp.Department,
                    designation = emp.Designation,
                    mobileNumber = emp.MobileNumber,
                    email = emp.Email,
                    role = emp.Role,
                    canChangePassword = true
                }, JsonRequestBehavior.AllowGet);
            }

            var qmsEmp = _qmsDb.EmployeeMasters.FirstOrDefault(e => e.EmployeeNo == empNo);
            if (qmsEmp != null)
            {
                return Json(new
                {
                    success = true,
                    fullName = qmsEmp.EmployeeName,
                    employeeNumber = qmsEmp.EmployeeNo,
                    department = qmsEmp.Department,
                    designation = qmsEmp.Designation,
                    mobileNumber = (string)null,
                    email = (string)null,
                    role = "Employee",
                    canChangePassword = true  
                }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { success = false, message = "Profile not found." }, JsonRequestBehavior.AllowGet);
        }

        // ── POST: /Profile/ChangePassword ──────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            string empNo = Session["EmployeeNumber"]?.ToString();
            if (string.IsNullOrEmpty(empNo))
                return Json(new { success = false, message = "Session expired. Please log in again." });

            if (string.IsNullOrWhiteSpace(currentPassword) || string.IsNullOrWhiteSpace(newPassword))
                return Json(new { success = false, message = "All fields are required." });

            if (newPassword.Length < 6)
                return Json(new { success = false, message = "New password must be at least 6 characters." });

            if (newPassword != confirmPassword)
                return Json(new { success = false, message = "New password and confirmation do not match." });

            if (currentPassword == newPassword)
                return Json(new { success = false, message = "New password must be different from the current password." });

            // 1) Try Autobase's own Employees table first (hashed password).
            var emp = _db.Employees.FirstOrDefault(e => e.EmployeeNumber == empNo);
            if (emp != null)
            {
                if (!PasswordHelper.VerifyPassword(currentPassword, emp.PasswordHash))
                    return Json(new { success = false, message = "Current password is incorrect." });

                emp.PasswordHash = PasswordHelper.HashPassword(newPassword);
                _db.SaveChanges();

                return Json(new { success = true, message = "Password changed successfully." });
            }
          
            var qmsEmp = _qmsDb.EmployeeMasters.FirstOrDefault(e => e.EmployeeNo == empNo);
            if (qmsEmp != null)
            {
                if (qmsEmp.Password != currentPassword)
                    return Json(new { success = false, message = "Current password is incorrect." });

                try
                {
                    qmsEmp.Password = newPassword;
                    _qmsDb.SaveChanges();
                    return Json(new { success = true, message = "Password changed successfully." });
                }
                catch (Exception)
                {                    
                    return Json(new { success = false, message = "Unable to update password right now. Please contact IT support." });
                }
            }

            return Json(new { success = false, message = "Account not found." });
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