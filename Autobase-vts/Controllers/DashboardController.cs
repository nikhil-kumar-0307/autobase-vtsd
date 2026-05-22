// Controllers/DashboardController.cs
using System.Web.Mvc;

namespace autobase.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        // GET: /Dashboard  — Admin & SuperAdmin
        [HttpGet]
        public ActionResult Index()
        {
            string role = Session["Role"]?.ToString();

            // Employees must not access the admin dashboard
            if (role == "Employee")
                return RedirectToAction("Employee");

            return View();   // Views/Dashboard/Index.cshtml  (uses _AdminLayout)
        }

        // GET: /Dashboard/Employee  — Employee-only
        [HttpGet]
        public ActionResult Employee()
        {
            string role = Session["Role"]?.ToString();

            // Admins/SuperAdmins sent to admin dashboard
            if (role == "Admin" || role == "SuperAdmin")
                return RedirectToAction("Index");

            return View("EmployeeDashboard");  // Views/Dashboard/EmployeeDashboard.cshtml
        }
    }
}