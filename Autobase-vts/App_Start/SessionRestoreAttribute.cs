using System.Web;
using System.Web.Mvc;
using autobase.Data;
using System.Linq;

namespace autobase.Filters
{
    public class SessionRestoreAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var session = filterContext.HttpContext.Session;
            var user = filterContext.HttpContext.User;

            // Only restore if authenticated but session is empty
            if (user.Identity.IsAuthenticated && session["Role"] == null)
            {
                var db = new AutobaseDbContext();
                var emp = db.Employees
                    .FirstOrDefault(e => e.EmployeeNumber == user.Identity.Name && e.IsActive);

                if (emp != null)
                {
                    session["Role"] = emp.Role;
                    session["FullName"] = emp.FullName;
                    session["EmployeeNumber"] = emp.EmployeeNumber;
                    session["EmployeeId"] = emp.Id;
                }
                else
                {
                    // Cookie exists but employee not found — force logout
                    System.Web.Security.FormsAuthentication.SignOut();
                    session.Clear();
                    filterContext.Result = new RedirectResult("/Account/Login");
                    return;
                }
            }

            base.OnActionExecuting(filterContext);
        }
    }
}