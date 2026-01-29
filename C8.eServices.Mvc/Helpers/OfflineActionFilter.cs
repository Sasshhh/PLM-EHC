using System.Net;
using System.Web.Mvc;

namespace C8.eServices.Mvc.Helpers
{
    public class OfflineActionFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.IsChildAction)
            {
                filterContext.Result = new ContentResult { Content = string.Empty };
            }
            else
            {
                var response = filterContext.HttpContext.Response;

                filterContext.Result = new ViewResult { ViewName = "_Offline" };
                response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                response.TrySkipIisCustomErrors = true;
            }

            //base.OnActionExecuting(filterContext);
        }
    }
}