using System.Web.Mvc;
using autobase.Filters;

namespace autobase.Controllers
{
    [Authorize]
    [SessionRestore]
    public class DashboardController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            string role = Session["Role"]?.ToString();

            if (role == "Employee")
                return RedirectToAction("EmployeeDashboard");

            return View();
        }

        [HttpGet]
        public ActionResult EmployeeDashboard()
        {
            string role = Session["Role"]?.ToString();

            if (role == "Admin" || role == "SuperAdmin")
                return RedirectToAction("Index");

            return View();
        }
    }
}