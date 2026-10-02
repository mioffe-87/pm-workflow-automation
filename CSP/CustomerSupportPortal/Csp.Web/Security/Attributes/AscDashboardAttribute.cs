using Csp.Api.Security;
using Csp.Api.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Csp.Api.Model;

namespace Csp.Web.Security.Attributes
{
    [Obsolete]
    public class AscDashboardAttribute : AuthorizeAttribute
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
                    //Populate the permissions and current state if not already set
                    if (!cspPrincipal.ActivePermissions.Any())
                    {
                        var homeLogic = DependencyResolver.Current.GetService<IHomeLogic>();
                        var securityLogic = DependencyResolver.Current.GetService<ISecurityLogic>();
                        var currentStateService = DependencyResolver.Current.GetService<ICurrentStateService>();
                        var supportAccountLogic = DependencyResolver.Current.GetService<ISupportAccountLogic>();

                        var userAccount = cspPrincipal.UserAccount;
                        var defaultSupportAccountId = homeLogic.GetOrCreateDefaultSupportAccountId(userAccount.Id) ?? 0;
                        cspPrincipal.ActivePermissions = securityLogic.GetSupportAccountPermissionsForPrincipal(defaultSupportAccountId, userAccount);

                        var supportAccount = supportAccountLogic.GetSupportAccount(defaultSupportAccountId, userAccount.Id);
                        // Comments By LK On 08/27
                        // Remove Current State dependencies
                        //currentStateService.SetSupportAccount(supportAccount);
                    }

                        //Internal PANW - ASC Score card
                        if (cspPrincipal.HasPermission(Permission.ViewASCDashboardReport))
                        {
                            cspPrincipal.ActivePermissions.Add(Permission.ViewASCDashboardReport);
                        }
                        else
                        {
                            var ascDeviceRegistrationService = DependencyResolver.Current.GetService<IAscDeviceRegistrationService>();

                            var isMemberOfAsc = ascDeviceRegistrationService.IsMemberOfAsc(cspPrincipal.UserAccount.Id, cspPrincipal.SupportAccountIdForPermissionSet != null ? Convert.ToInt64(cspPrincipal.SupportAccountIdForPermissionSet) : 0);

                            if (!isMemberOfAsc)
                            {
                                
                                filterContext.Result =
                                    new RedirectToRouteResult(
                                         new RouteValueDictionary(new { controller = "Error", action = "UnAuthorized" }));
                            }
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