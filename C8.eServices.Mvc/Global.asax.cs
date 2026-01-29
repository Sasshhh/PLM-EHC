using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using C8.eServices.Mvc.DataAccessLayer;
using System.Web.Helpers;
using System.Security.Claims;
using System.Web.Http;
using C8.eServices.Mvc.Helpers;

namespace C8.eServices.Mvc
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure( WebApiConfig.Register );
            GlobalConfiguration.Configuration.Formatters.JsonFormatter.UseDataContractJsonSerializer = false;

            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            //GlobalFilters.Filters.Add(new SqlExceptionFilter());

            OfflineData.Execute(
                online: () =>
                {
                   //DatabaseConfiguration.Start();
                },
                offline: () =>
                {
                    GlobalFilters.Filters.Add(new OfflineActionFilter());
                    //FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
                });

            AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.NameIdentifier;
            System.Web.Optimization.PreApplicationStartCode.Start();

            MvcHandler.DisableMvcResponseHeader = true;
        }

        /// <summary>
        /// NC.20161009
        /// Prevents back button after logout.
        /// </summary>
        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            var app = sender as HttpApplication;

            if (app != null && app.Context != null)
                app.Context.Response.Headers.Remove("Server");

            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.AppendCacheExtension("no-store, must-revalidate");
            Response.AppendHeader("Pragma", "no-cache");
            Response.AppendHeader("Expires", "0");

            //Response.Cache.SetCacheability(HttpCacheability.ServerAndNoCache);
            //Response.Cache.SetExpires(DateTime.UtcNow.AddHours(-1));
            //Response.Cache.SetNoStore();
        }

        //Secure AntiForgeryTokens
        private static void ConfigureAntiForgeryTokens()
        {
            // Rename the Anti-Forgery cookie from "__RequestVerificationToken" to "f". 
            // This adds a little security through obscurity and also saves sending a 
            // few characters over the wire.
            AntiForgeryConfig.CookieName = "es";

            // If you have enabled SSL. Uncomment this line to ensure that the Anti-Forgery 
            // cookie requires SSL to be sent accross the wire. 
            AntiForgeryConfig.RequireSsl = true;
        }
    }
}
