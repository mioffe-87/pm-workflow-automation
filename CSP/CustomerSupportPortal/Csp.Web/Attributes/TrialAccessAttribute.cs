using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Csp.Api.Services.Contracts;
using Csp.Web.HelperClasses;

namespace Csp.Web.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class TrialAccessAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var hasMembership = CurrentState.HasDirectMembership();
            var isSupportOnlyAccount = SiteHelper.IsSupportOnlyAccount();

            if (!hasMembership && isSupportOnlyAccount)
            {
                var routeDictionary = new RouteValueDictionary(new { controller = "Error", action = "UnAuthorized" });
                filterContext.Result = new RedirectToRouteResult(routeDictionary);
                base.OnActionExecuting(filterContext);
            }

        }
    }
}