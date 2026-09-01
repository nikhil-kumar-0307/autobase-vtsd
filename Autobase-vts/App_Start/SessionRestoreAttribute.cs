using System.Linq;
using System.Web.Mvc;
using autobase.Data;

namespace autobase.Filters
{
    /// <summary>
    /// Runs as a global authorization filter (before RoleAuthorize on any action)
    /// to repopulate Session[...] from the DB when the forms-auth cookie is valid
    /// but session has expired (e.g. RememberMe case).
    /// </summary>
    public class SessionRestoreAttribute : FilterAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            var session = filterContext.HttpContext.Session;
            var user = filterContext.HttpContext.User;

            if (user.Identity.IsAuthenticated && session?["Role"] == null)
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
                    // Cookie exists but employee not found / deactivated — force logout
                    System.Web.Security.FormsAuthentication.SignOut();
                    session.Clear();
                    filterContext.Result = new RedirectResult("~/Account/Login");
                }
            }
        }
    }
}