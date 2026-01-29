using System.Web.Http;
using System.Web.Http.Cors;

namespace C8.eServices.Mvc
{
    public static class WebApiConfig
    {
        public static void Register( HttpConfiguration config )
        {
            // TODO: Add any additional configuration code.

            config.Formatters.JsonFormatter
                .SerializerSettings
                .ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;

            // Web API routes
            config.MapHttpAttributeRoutes();

            var cors = new EnableCorsAttribute( "*", "*", "*" );
            cors.SupportsCredentials = true;
            config.EnableCors( cors );

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            

            // WebAPI when dealing with JSON & JavaScript!
            // Setup json serialization to serialize classes to camel (std. Json format)
            var formatter = GlobalConfiguration.Configuration.Formatters.JsonFormatter;
            formatter.SerializerSettings.ContractResolver =
                new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();
        }
    }
}
