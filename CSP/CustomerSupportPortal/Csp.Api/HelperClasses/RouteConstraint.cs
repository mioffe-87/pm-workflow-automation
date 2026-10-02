using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Csp.Api.HelperClasses
{
    public class RouteConstraint : IRouteConstraint
    {
        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
        {

            if (routeDirection == RouteDirection.IncomingRequest && parameterName == "id")
            {
                // id param is empty
                if (values["id"] == UrlParameter.Optional)
                    return true;
                int id;
                if (Int32.TryParse(values["id"].ToString(), out id))
                    return true;
            }
            return false;
        }
    }
}
