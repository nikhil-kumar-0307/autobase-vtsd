using System.Web;
using System.Web.Mvc;

namespace autobase.Helpers
{
    /// <summary>
    /// Restricts a controller/action to specific roles stored in Session["Role"].
    /// Falls back to standard FormsAuthentication redirect if not authenticated.
    /// </summary>
    public class RoleAuthorizeAttribute : AuthorizeAttribute
    {
        private readonly string[] _allowedRoles;

        public RoleAuthorizeAttribute(params string[] roles)
        {
            _allowedRoles = roles;
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (!httpContext.Request.IsAuthenticated)
                return false;

            var sessionRole = httpContext.Session?["Role"]?.ToString();
            if (string.IsNullOrEmpty(sessionRole))
                return false;

            foreach (var role in _allowedRoles)
                if (sessionRole == role)
                    return true;

            return false;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (!filterContext.HttpContext.Request.IsAuthenticated)
            {
                // Not logged in → login page
                base.HandleUnauthorizedRequest(filterContext);
            }
            else
            {
                // Logged in but wrong role → 403 Forbidden view
                filterContext.HttpContext.Response.StatusCode = 403;
                filterContext.Result = new ViewResult { ViewName = "Unauthorized" };
            }
        }
    }
}