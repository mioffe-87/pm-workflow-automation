using Csp.Api.Model;
using Csp.Api.Security;
using Csp.Web.DataTransferObjects;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Csp.Api.Services.Contracts;
using System;
using System.Net.Http;
using System.Web.Security;
using Csp.Web.HelperClasses;

namespace Csp.Web.Controllers {

	public class BaseController : Controller {

		public BaseController() {

		}

		#region Private Members

		private ISecurityLogic _securityLogic;
		private IGroupLogic _groupLogic;
		private IMembershipLogic _membershipLogic;
		private ISupportAccountLogic _supportAccountLogic;
		private ILicenseLogic _licenseLogic;
		private IUserAccountLogic _userAccountLogic;
		private ILookupLogic _lookupLogic;
        private ISaasLogic _saasLogic;

		private HttpContextBase Context { get; set; }

		#endregion

		#region Protected Members

		protected override void Initialize(RequestContext requestContext) {


            Context = requestContext.HttpContext;
			Context.Response.Cache.SetAllowResponseInBrowserHistory(false);
			Context.Response.Cache.SetCacheability(HttpCacheability.NoCache);
			Context.Response.Cache.SetNoStore();
			Context.Response.Expires = 0;
			Context.Response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
			base.Initialize(requestContext);

			SavePermissions(requestContext);
		}

		private void SavePermissions(RequestContext requestContext) {

			var controller = TryReadRoute(requestContext, "controller");
			var action = TryReadRoute(requestContext, "action");
			var id = TryReadRoute(requestContext, "id");
			var supportAccount = new SupportAccount();
			var group = new Group();
			var device = new Device();
            long lId;
            //if there are keys in the request, then that means there is a model data and that should be updated for easy access to the Id.
            var isModelInReqest = (requestContext.HttpContext.Request.Form.Keys.Count > 0);

			/* If a support account or group is passed we must use that to update the current state to ensure the names
				 * are in synch with data that is updated.*/

			//This updates the model objects with the key values for simplified access.
			/*if (isModelInReqest) {
				TryUpdateModel(supportAccount, "SupportAccount");
				TryUpdateModel(group, "Group");
				TryUpdateModel(device, "Device");
			}*/

			var permissionService = DependencyResolver.Current.GetService<IPermissionService>();
            Int64.TryParse(id, out lId);

            // Comments By LK On 08.26
            // Remove CurrentState dependencies

            /*var cspPrincipal = permissionService.GetSessionDetails(lId);

            if (User != null && User is CspPrincipal)
            {
                CurrentState.SetLoggedInUser((CspPrincipal)HttpContext.User);
            }

            permissionService.UpdateCurrentStateAndPermissions(cspPrincipal, controller, action, id, supportAccount, group, device);*/

            permissionService.SetCurrentPrincipal(controller, action, lId);
        }

		private string TryReadRoute(RequestContext requestContext, string routeName) {

			var value = requestContext.RouteData.Values[routeName];

			return value == null ? null : value.ToString();
		}

		/// <summary>
		/// These protected properties will soon be removed due to dependency injection. This is based on the initial design which did not include a DI container
		/// </summary>
		protected ISupportAccountLogic SupportAccountLogic {

			get { return _supportAccountLogic ?? (_supportAccountLogic = DependencyResolver.Current.GetService<ISupportAccountLogic>()); }
		}

		protected IUserAccountLogic UserAccountLogic {

			get { return _userAccountLogic ?? (_userAccountLogic = DependencyResolver.Current.GetService<IUserAccountLogic>()); }
		}

		protected IGroupLogic GroupLogic {

			get { return _groupLogic ?? (_groupLogic = DependencyResolver.Current.GetService<IGroupLogic>()); }
		}

		protected ISecurityLogic SecurityLogic {

			get { return _securityLogic ?? (_securityLogic = DependencyResolver.Current.GetService<ISecurityLogic>()); }
		}

		protected IMembershipLogic MembershipLogic {

			get { return _membershipLogic ?? (_membershipLogic = DependencyResolver.Current.GetService<IMembershipLogic>()); }
		}

		protected ILicenseLogic LicenseLogic {

			get { return _licenseLogic ?? (_licenseLogic = DependencyResolver.Current.GetService<ILicenseLogic>()); }
		}

		protected ILookupLogic LookupLogic {

			get { return _lookupLogic ?? (_lookupLogic = DependencyResolver.Current.GetService<ILookupLogic>()); }
		}

        protected ISaasLogic SaasLogic
        {

            get { return _saasLogic ?? (_saasLogic = DependencyResolver.Current.GetService<ISaasLogic>()); }
        }

		/// <summary>
		/// This allows programatic access to the CspPrincipal. 
		/// </summary>
		protected virtual new CspPrincipal User {

			get { return HttpContext.User as CspPrincipal; }
		}

        protected bool VerifyAuthorizationBySupportAccount(long supportAccountId)
        {
            return SaasLogic.VerifyAuthorizationBySupportAccount(supportAccountId, HttpContext.User.Identity.Name);
        }

        protected bool VerifyAuthorizationBySerailNumber(string serialNumber)
        {
            return SaasLogic.VerifyAuthorizationBySerailNumber( serialNumber, HttpContext.User.Identity.Name);
        }
		#endregion

		#region Public Members

		public JsonResult GetCountries() {
			var countries = LookupLogic.GetCountries();
			ViewBag.CountryList = countries;

			return Json(countries, JsonRequestBehavior.AllowGet);
		}

		public JsonResult GetRegions(int countryId) {
			var regions = LookupLogic.GetRegions(countryId);
			ViewBag.RegionList = regions;

			return Json(regions, JsonRequestBehavior.AllowGet);
		}

		public ActionResult CspHelp(int id) {
			var helpContent = LookupLogic.GetCspHelpById(id);
			var model = new CspHelpDto() { Content = helpContent };

			return PartialView("../Partial/_CspHelp", model);
		}

		#endregion
	}
}
