using Csp.Api.Security;
using Csp.Api.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Providers.Entities;
using System.Web.Routing;
using Csp.Web.HelperClasses;

namespace Csp.Web.Security.Attributes
{

    public class BulkRegistrationAuthorizeAttribute : AuthorizeAttribute
    {

        /// <summary>
        /// This method is overriden to allow for custom authorization
        /// </summary>
        /// <param name="filterContext">The authorization context information</param>
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            try
            {
                //Gets the custom principal from the HttpContext.
                CspPrincipal cspPrincipal = HttpContext.Current.User as CspPrincipal;

                /*If the principal is not null then check to see if the user has permission in the 
                 * permission set to take the action. If not then redirect to a Not Authorized page.
                 */
                if (cspPrincipal != null)
                {
                    var siteLicenseLogic = DependencyResolver.Current.GetService<ISiteLicenseLogic>();
                    var hasBulkRegistrationAccess = cspPrincipal.HasPermission(Permission.ViewBulkRegistration);
                    //var hasEaLicense = siteLicenseLogic.HasEaLicense(CurrentState.GetSupportAccountId() ?? 0);

                    if (!hasBulkRegistrationAccess)
                    {
                        filterContext.Result =
                            new RedirectToRouteResult(
                                    new RouteValueDictionary(new { controller = "Error", action = "UnAuthorized" }));
                    }
                }
                else
                {
                    base.OnAuthorization(filterContext);
                }
            }
            catch (Exception)
            {
                filterContext.Result =
                                   new RedirectToRouteResult(
                                       new RouteValueDictionary(new { controller = "Home", action = "NotAuthorized" }));
            }
        }
    }
}