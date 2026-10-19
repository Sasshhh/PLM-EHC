using System;
using System.Web.Mvc;
using System.Web.Routing;
using C8.eServices.Mvc.Helpers;

namespace C8.eServices.Mvc.Filters
{
    /// <summary>
    /// Restricts a controller (or action) to users belonging to one or more
    /// departments, identified by their ApplicationEntities [Key].
    ///
    /// Hiding a menu is presentation, not security: during factory testing an EHC
    /// Property Manager whose Real Estate menus were correctly hidden could still
    /// open /RealEstateAdmin/Index by typing the address, and the page rendered the
    /// live Real Estate master data with working Add/Edit/Delete controls. This
    /// filter closes that gap on the server.
    ///
    /// Usage:
    ///     [DepartmentAuthorize(ApplicationEntityKeys.RealEstateDevelopment)]
    ///     public class RealEstateAdminController : Controller { ... }
    ///
    /// Cross-department administrators (Super Administrators, Administrators,
    /// Support Admin) are always allowed. Note that "Back Office System
    /// Administrator" is NOT treated as cross-department, because both EHC and
    /// Real Estate use that role name.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class DepartmentAuthorizeAttribute : ActionFilterAttribute
    {
        private readonly string[] _allowedDepartmentKeys;

        public DepartmentAuthorizeAttribute(params string[] allowedDepartmentKeys)
        {
            _allowedDepartmentKeys = allowedDepartmentKeys ?? new string[0];
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            try
            {
                if (filterContext == null || filterContext.HttpContext == null) return;

                var request = filterContext.HttpContext.Request;
                if (!request.IsAuthenticated)
                {
                    // let the normal [Authorize] pipeline deal with anonymous users
                    base.OnActionExecuting(filterContext);
                    return;
                }

                if (DepartmentHelper.IsCrossDepartmentAdmin()) return;
                if (DepartmentHelper.IsInDepartment(_allowedDepartmentKeys)) return;

                // Wrong department: refuse, and say so rather than silently 404ing.
                if (request.IsAjaxRequest())
                {
                    filterContext.Result = new HttpStatusCodeResult(403,
                        "This area belongs to another department.");
                    return;
                }

                filterContext.Controller.TempData["MessageTitle"] = "Not available to your department";
                filterContext.Controller.TempData["Message"] =
                    "This area belongs to another department and is not available to your account. " +
                    "If you believe this is wrong, ask your administrator to check the department " +
                    "assigned to your user profile.";

                // Back to the PLM landing page. This used to be Home/Index, which is the
                // municipal Siyakhokha portal page - so a refused EHC user was dropped out
                // of PLM entirely onto the City of Ekurhuleni home.
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary(new { controller = DepartmentHelper.LandingController,
                                                   action = DepartmentHelper.LandingAction }));
            }
            catch (Exception)
            {
                // A fault in the guard must never take the site down; fall through to
                // the action, which still has its own [Authorize] protection.
            }

            base.OnActionExecuting(filterContext);
        }
    }
}
