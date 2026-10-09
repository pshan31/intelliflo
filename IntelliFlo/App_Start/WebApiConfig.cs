using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace IntelliFlo
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            config.Formatters.XmlFormatter.SupportedMediaTypes.Remove(
config.Formatters.XmlFormatter.SupportedMediaTypes.FirstOrDefault(t => t.MediaType == "application/xml"));

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "services/{controller}",
                defaults: new { id = RouteParameter.Optional }
            );

            //config.Routes.MapHttpRoute(
            //    name: "CatchAllRoutes",
            //    routeTemplate: "{*url}",
            //    defaults: new { controller = "Default", action = "HandleAll" }
            //);
        }
    }
}
