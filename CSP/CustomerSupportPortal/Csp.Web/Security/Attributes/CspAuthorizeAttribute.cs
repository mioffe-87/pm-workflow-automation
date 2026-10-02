using System;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Csp.Api.Security;
using System.Web.Routing;
using Csp.Api.HelperClasses;
using Csp.Api.Model;
using Csp.Api.Model.Enums;
using Csp.Api.Services.Contracts;
using Csp.Web.HelperClasses;

namespace Csp.Web.Security.Attributes {
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
	class CspAuthorizeAttribute : AuthorizeAttribute {

		/// <summary>
		/// 
		/// </summary>
		public Permission Permission {
			get;
			set;
		}

		/// <summary>
		/// This custom authorization attribute takes a permission enum as a parameter. 
		/// The attribute is applied to a action method of a controller to check and see
		/// if the user is authorized to access it. 
		/// </summary>
		/// <param name="permission"></param>
		public CspAuthorizeAttribute(Permission permission) {
			Permission = permission;
		}

		/// <summary>
		/// This method is overriden to allow for custom authorization
		/// </summary>
		/// <param name="filterContext">The authorization context information</param>
		public override void OnAuthorization(AuthorizationContext filterContext) {
			var httpContext = filterContext.HttpContext;
			var request = httpContext.Request;
			var response = httpContext.Response;
			//Gets the custom principal from the HttpContext.
			CspPrincipal cspPrincipal = HttpContext.Current.User as CspPrincipal;

			/*If the principal is not null then check to see if the user has permission in the 
			 * permission set to take the action. If not then redirect to a Not Authorized page.
			 */
			if (cspPrincipal != null) {
				if ((!cspPrincipal.HasPermission(Permission)) || !HasAccess() || !CheckFedrampAccess()) {

					if (request.IsAjaxRequest())
					{
						response.StatusCode = (int)HttpStatusCode.Unauthorized;
						response.SuppressFormsAuthenticationRedirect = true;
						response.End();
					}

                    bool isExpired = cspPrincipal.UserAccount?.IsMembershipExpired == null ? false : cspPrincipal.UserAccount.IsMembershipExpired;
                    bool isPending = (cspPrincipal.UserAccount?.MembershipStatus?.ToLower() == MembershipStatus.Pending.ToString().ToLower()) ? true : false;
                    bool isValid = cspPrincipal.UserAccount?.IsValid == null ? false : cspPrincipal.UserAccount.IsValid;

                    filterContext.Result =
						new RedirectToRouteResult(
							new RouteValueDictionary(new { controller = "Error", action = "UnAuthorizedAccess", isExpired, isPending, isValid }));
				}
			} else {
				base.OnAuthorization(filterContext);
			}
		}

        private bool  HasAccess()
        {
            try
            {
                var userInfo = UserHelper.GetSsoToken();
                if (userInfo != null)
                {
                    bool isImpersonated = userInfo["isimpersonated"] != null && userInfo["isimpersonated"].Equals("true");
                    string impersonatorEmail = Convert.ToString(userInfo["impersonatoremail"]);

                    if (isImpersonated && !string.IsNullOrEmpty(impersonatorEmail))
                    {
                        var userAccountLogic = DependencyResolver.Current.GetService<IUserAccountLogic>();
                        var impersonateRequestLogic = DependencyResolver.Current.GetService<IImpersonateRequestLogic>();

                        //check if user has approved impersonate request to the current account
                        UserAccount impersonatorUser = userAccountLogic.GetUserAccountByEmail(impersonatorEmail);
                        if (impersonatorUser != null)
                        {
                            string requestId = string.Empty;
                            bool hasAccessToImpersonate = impersonateRequestLogic.HasValidImpersonateRequest(new ImpersonateRequest()
                            {
                                TargetUserAccountId = CurrentState.GetLoggedInUser().UserAccount.Id,
                                TargetSupportAccountId = CurrentState.GetSupportAccountId() ?? 0,
                                RequestorUserAccountId = impersonatorUser.Id,
                                ImpersonateAction = ImpersonateAction.ValidRequests.ToDescription()
                            }, out requestId);

                            return hasAccessToImpersonate;

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return false;
                //throw;
            }

            return true;
        }

        private bool CheckFedrampAccess()
        {
            try
            {
                if (CurrentState.GetSupportAccount().IsFedramp)
                {
                   var hasFedrampClassification =  CurrentState.GetLoggedInUser().RoleSecurities.Find(x => x.SecurityClassification == RoleSecurity.SecurityClassifications.Fedramp);
                   return hasFedrampClassification != null || CurrentState.HasDirectMembership();
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return false;
            }

            return true;
        }

    }
}
