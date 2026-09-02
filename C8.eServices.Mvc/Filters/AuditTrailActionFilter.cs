using System;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Helpers;

namespace C8.eServices.Mvc.Filters
{
    /// <summary>
    /// Global MVC action filter that automatically logs every controller action
    /// to the audit trail. Wrapped in try/catch — never throws.
    /// </summary>
    public class AuditTrailActionFilter : ActionFilterAttribute
    {
        // Skip logging for these controllers/actions to avoid noise
        private static readonly string[] SkipControllers = new[]
        {
            "Error", "Elmah", "Content", "Scripts", "Bundles"
        };

        private static readonly string[] SkipActions = new[]
        {
            "HeartBeat", "Ping", "KeepAlive"
        };

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            try
            {
                if (filterContext == null || filterContext.HttpContext == null)
                    return;

                // Only log authenticated user actions
                if (!filterContext.HttpContext.Request.IsAuthenticated)
                    return;

                string controllerName = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
                string actionName = filterContext.ActionDescriptor.ActionName;

                // Skip noise controllers
                if (SkipControllers.Any(sc => controllerName.Equals(sc, StringComparison.OrdinalIgnoreCase)))
                    return;

                // Skip noise actions
                if (SkipActions.Any(sa => actionName.Equals(sa, StringComparison.OrdinalIgnoreCase)))
                    return;

                // Skip AJAX/JSON partial requests to avoid flooding
                if (filterContext.HttpContext.Request.IsAjaxRequest())
                    return;

                // Skip child actions (partial views)
                if (filterContext.IsChildAction)
                    return;

                // Resolve current user's SystemUserId
                int? systemUserId = ResolveCurrentUserId(filterContext.HttpContext);

                // Determine activity type based on HTTP method
                string method = filterContext.HttpContext.Request.HttpMethod;
                string activityType = "PageView";
                if (method == "POST" || method == "PUT" || method == "DELETE")
                    activityType = "FormSubmit";

                // Build description
                string description = string.Format("{0} {1}/{2}",
                    method, controllerName, actionName);

                // Log the activity
                AuditTrailHelper.LogActivity(
                    systemUserId: systemUserId,
                    controller: controllerName,
                    action: actionName,
                    description: description,
                    activityType: activityType
                );
            }
            catch (Exception)
            {
                // Swallow — audit filter must never break the request pipeline
            }
        }

        /// <summary>
        /// Resolves the current SystemUser.Id from the authenticated identity.
        /// </summary>
        private int? ResolveCurrentUserId(HttpContextBase httpContext)
        {
            try
            {
                if (httpContext == null || httpContext.User == null || !httpContext.User.Identity.IsAuthenticated)
                    return null;

                string username = httpContext.User.Identity.Name;
                if (string.IsNullOrEmpty(username))
                    return null;

                using (var db = new eServicesDbContext())
                {
                    var user = db.SystemUsers.FirstOrDefault(u => u.UserName == username);
                    return user?.Id;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
