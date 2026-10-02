using System.Web.Mvc;
using System.Web.Routing;
using Csp.Api.HelperClasses;

namespace Csp.Web {
	public class RouteConfig {
		public static void RegisterRoutes(RouteCollection routes) {
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("{resource}.asmx/{*pathInfo}");

            // BotDetect requests must not be routed
            routes.IgnoreRoute("{*botdetect}", new { botdetect = @"(.*)BotDetectCaptcha\.ashx" });

            routes.MapRoute(
                name: "Returns", 
                url:"PA200ValidationTool",
                defaults:
					new { controller = "Returns", action = "Index", id = UrlParameter.Optional, tabIndex = UrlParameter.Optional }
				);

            routes.MapRoute(
                name: "Activate",
                url: "activate/flex/{id}",
                defaults:
                    new { controller = "SupportAccount", action = "FirewallFlex", id = UrlParameter.Optional }
                );

            routes.MapRoute(
                name: "ActivateWfApi",
                url: "activate/wfapi/{id}",
                defaults:
                    new { controller = "Wildfire", action = "Activate", id = UrlParameter.Optional }
                );

            routes.MapRoute(
               name: "Search",
               url: "search/{id}",
               defaults:
                   new { controller = "Search", action = "Index", id = UrlParameter.Optional }
                );

            routes.MapRoute(
              name: "Updates",
              url: "Updates/{id}",
              defaults:
                  new { controller = "Updates", action = "Index", id = UrlParameter.Optional },
                  constraints: new { id = new RouteConstraint() }
               );
            
            routes.MapRoute(
				name: "Default",
				url: "{controller}/{action}/{id}/{tabIndex}",
				defaults:
					new { controller = "Home", action = "Index", id = UrlParameter.Optional, tabIndex = UrlParameter.Optional }
				);
		}
	}
}