using System.Web;
using System.Web.Mvc;
using autobase.Filters;

namespace Autobase_vts
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new SessionRestoreAttribute());
        }
    }
}
