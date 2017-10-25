using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace youtube
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
            name: "Hyperlink",
            url: "Supermarkets/Paddy/{search}",
            defaults: new { controller = "Supermarkets", action = "search", search = UrlParameter.Optional }
        );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{search}",
                defaults: new { controller = "Supermarkets", action = "search", search = UrlParameter.Optional}
            );

            //routes.MapRoute(
            //            name: "Page",
            //            url: "{supermarketItems}.aspx",
            //            defaults: new { controller = "Supermarkets", action = "search", search = UrlParameter.Optional }
            //        );

            //        routes.MapRoute(
            //    name: "AspxRoute",
            //    url: "{permalink}.aspx",
            //    defaults: new { controller = "LegacyRedirection", action = "Aspx" }
            //);

        }
    }
}
