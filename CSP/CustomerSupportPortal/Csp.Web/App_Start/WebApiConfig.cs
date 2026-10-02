using Csp.Api.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Routing;
using System.Web.Mvc;

namespace Csp.Web
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            #region Enable CORS
            string AllowedOrigins = GetAllowedOrigins();
            var cors = new EnableCorsAttribute(AllowedOrigins, "*", "*");
            cors.SupportsCredentials = true;
            config.EnableCors(cors);
            #endregion

            /*config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );*/

            config.Routes.MapHttpRoute("DefaultApiWithId", "Api/{controller}/{id}", new { id = RouteParameter.Optional }, new { id = @"\d+" });
            config.Routes.MapHttpRoute("DefaultApiWithAction", "Api/{controller}/{action}");
            config.Routes.MapHttpRoute("DefaultApiGet", "Api/{controller}", new { action = "Get" }, new { httpMethod = new HttpMethodConstraint(HttpMethod.Get) });
            config.Routes.MapHttpRoute("DefaultApiPost", "Api/{controller}", new { action = "Post" }, new { httpMethod = new HttpMethodConstraint(HttpMethod.Post) });

            config.Routes.MapHttpRoute("Proxy", "proxy/{action}/{*path}", new { controller = "Services" });
            
            // Uncomment the following line of code to enable query support for actions with an IQueryable or IQueryable<T> return type.
            // To avoid processing unexpected or malicious queries, use the validation settings on QueryableAttribute to validate incoming queries.
            // For more information, visit http://go.microsoft.com/fwlink/?LinkId=279712.
            //config.EnableQuerySupport();

            // To disable tracing in your application, please comment out or remove the following line of code
            // For more information, refer to: http://www.asp.net/web-api
            config.EnableSystemDiagnosticsTracing();

            if (Convert.ToBoolean(ConfigurationManager.AppSettings["ExcludeHelpPage"]))
            {
                config.Routes.IgnoreRoute("help", "help");
            }
        }

        public static string GetAllowedOrigins()
        {
            return ConfigurationManager.AppSettings["AllowedOrigins"].ToString();
        }
    }
}
