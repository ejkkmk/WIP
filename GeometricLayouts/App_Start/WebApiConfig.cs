using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace GeometricLayouts
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );


            config.Routes.MapHttpRoute(
                name: "GetCoordinatesRowCol",
                routeTemplate: "api/{controller}/{RowString}/{ColString}"
            );

            config.Routes.MapHttpRoute(
                name: "GetRowCol",
                routeTemplate: "api/{controller}/{Ptx1}/{Pty1}/{Ptx2}/{Pty2}/{Ptx3}/{Pty3}/"
            );

        }
    }
}
