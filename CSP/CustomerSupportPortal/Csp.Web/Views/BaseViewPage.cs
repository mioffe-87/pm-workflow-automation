using System;
using System.Linq;
using Csp.Api.Security;
using System.Collections.Generic;
using System.Web.Mvc;
using Csp.Api.Model;
using Csp.Api.Services.Contracts;
using Csp.Web.HelperClasses;

namespace Csp.Web.Views {

	public abstract class BaseViewPage<TModel> : WebViewPage<TModel> {

		protected BaseViewPage() {

			//This property is meant to only be instantiated privately to ensure all Ids are unique.
			UniqueId = new DynamicUniqueHtmlAttrIds();
		}

		/// <summary>
		/// This is the User that is logged into the system. When information about the user, 
		/// such as permissions or user account information, this property should be accessed. 
		/// </summary>
		public virtual new CspPrincipal User {
			get {
				return base.User as CspPrincipal;
			}
		}

		/// <summary>
		/// This dynamic property is read only when used and is meant to be used to generate a list of Unique IDs 
		/// that can be used as the ID for an HTML tag or a prefix to a embedded javascript function or variable. 
		/// To use it simply access the object in the view and choose any property name of the UniqueID object 
		/// such as UniqueID.MyId or UniqueID.putAnyNameYouLikeHere. 
		/// This call to read the property dynamically also creates the id in a unique list for the name
		/// of the property you defined. The actual Id that is read from it is the name you provide in lower case
		/// plus a guid to ensure it is unique for each view that calls it. The class used to implement this 
		/// functionality is Csp.Web.HelperClasses.DynamicUniqueHtmlAttrIds.
		/// Please see comments for further information if needed on that class. 
		/// </summary>
		public dynamic UniqueId {
			get;
			private set;

		}

	    //get {
		    //    var jumpToItems = new List<SelectListItem>();

		    //    var u = new UrlHelper(Url.RequestContext);

		    //    if (CurrentState.IsAccountLoaded()) {

		    //        // Get the SupportAccount ID of the current User
		    //        var supportAccountId = CurrentState.GetSupportAccountId().ToString();

		    //        if (CurrentState.HasLoggedInPermission(Permission.ViewSupportAccount)) {
		    //            jumpToItems.Add(new SelectListItem { Text = Constants.HOME, Value = u.Action("MyAccounts", "SupportAccount", new { id = supportAccountId, data_link_type = "go-to", data_link_selection = Constants.HOME }) });
		    //        }
		    //        if (CurrentState.HasLoggedInPermission(Permission.ViewCustomerSupportAccounts)) {
		    //            jumpToItems.Add(new SelectListItem { Text = Constants.ACCOUNT, Value = u.Action("LoadAccountPage", "SupportAccount", new { id = supportAccountId, data_link_type = "go-to", data_link_selection = Constants.ACCOUNT }) });
		    //        }
		    //        if (CurrentState.HasLoggedInPermission(Permission.ViewSupportAccountMemberships)) {
		    //            jumpToItems.Add(new SelectListItem { Text = Constants.MEMBERS, Value = u.Action("AccountUsers", "SupportAccount", new { id = supportAccountId, data_link_type = "go-to", data_link_selection = Constants.MEMBERS }) });
		    //        }
		    //        if (CurrentState.HasLoggedInPermission(Permission.ViewSupportAccountDevices)) {
		    //            jumpToItems.Add(new SelectListItem { Text = Constants.DEVICES, Value = u.Action("AccountDevices", "SupportAccount", new { id = supportAccountId, data_link_type = "go-to", data_link_selection = Constants.DEVICES }) });
		    //        }
		    //        if (CurrentState.HasLoggedInPermission(Permission.ViewSupportAccountGroups)) {
		    //            jumpToItems.Add(new SelectListItem { Text = Constants.GROUPS, Value = u.Action("AccountGroups", "SupportAccount", new { id = supportAccountId, data_link_type = "go-to", data_link_selection = Constants.GROUPS }) });
		    //        }

		    //        jumpToItems.Add(new SelectListItem { Text = Constants.DropDownSpacer, Value = "" });

		    //        var showSpacer = false;

		    //        if (CurrentState.HasLoggedInPermission(Permission.CreateUserAccount)) {
		    //            showSpacer = true;
		    //            jumpToItems.Add(new SelectListItem { Text = "Create New User", Value = u.Action("Create", "UserAccount", new { id = CurrentState.GetSupportAccountId() ?? 0, data_link_type = "go-to", data_link_selection = "Create New User" }) });
		    //        }
		    //        if (CurrentState.HasLoggedInPermission(Permission.EditDeviceTags)) {
		    //            showSpacer = true;
		    //            jumpToItems.Add(new SelectListItem { Text = "Device Tags", Value = u.Action("DeviceCategories", "Device", new { id = CurrentState.GetSupportAccountId() ?? 0, data_link_type = "go-to", data_link_selection = "Device Tags" }) });
		    //        }
		    //        if (CurrentState.HasLoggedInPermission(Permission.RegisterDevice)) {
		    //            showSpacer = true;
		    //            jumpToItems.Add(new SelectListItem { Text = "Licensing API", Value = u.Action("LicensingApi", "License", new { id = CurrentState.GetSupportAccountId() ?? 0, data_link_type = "go-to", data_link_selection = "Licensing API" }) });
		    //        }
		    //        if (CurrentState.HasLoggedInPermission(Permission.EditWildfireUsers)) {
		    //            showSpacer = true;
		    //            jumpToItems.Add(new SelectListItem { Text = "Wildfire Users", Value = u.Action("WildfireUsers", "SupportAccount", new { id = CurrentState.GetSupportAccountId() ?? 0, data_link_type = "go-to", data_link_selection = "Wildfire Users" }) });
		    //        }

		    //        var siteLicenseLogic = DependencyResolver.Current.GetService<ISiteLicenseLogic>();
		    //        var hasBulkRegistrationAccess = User.HasPermission(Permission.ViewBulkRegistration);
		    //        var hasEaLicense = siteLicenseLogic.HasEaLicense(CurrentState.GetSupportAccountId() ?? 0);

		    //        if (hasBulkRegistrationAccess && !hasEaLicense) {
		    //            showSpacer = true;
		    //            jumpToItems.Add(new SelectListItem { Text = "Bulk Registration History", Value = u.Action("History", "BulkRegistration", new { id = CurrentState.GetSupportAccountId() ?? 0, data_link_type = "go-to", data_link_selection = "Bulk Registration History" }) });
		    //        }

		    //        if (showSpacer) {
		    //            jumpToItems.Add(new SelectListItem { Text = Constants.DropDownSpacer, Value = "" });
		    //        }
		    //    }

		    //    //var securityLogic = DependencyResolver.Current.GetService<ISecurityLogic>();
		    //    //var returnsLogic = DependencyResolver.Current.GetService<IReturnsLogic>();

		    //    //var supportAccountIdVal = CurrentState.GetSupportAccountId();
		    //    //var canSeeAutoFocusLink = supportAccountIdVal.HasValue
		    //    //    && securityLogic.CanSeeAutoFocusLink(CurrentState.GetLoggedInUser().UserAccount.Id, supportAccountIdVal.Value);

		    //    //var hasReturnsPortalAccess = supportAccountIdVal.HasValue && CurrentState.HasLoggedInPermission(Permission.ViewPA200SSDPortal)
		    //    //                             && returnsLogic.ShowReturnsPortalLink(supportAccountIdVal.Value, User.UserAccount.Id);

		    //    //var homeLogic = DependencyResolver.Current.GetService<IHomeLogic>();
		    //    //var resourceLinks = homeLogic.GetResourceLinks(CurrentState.HasLoggedInPermission(Permission.ViewSoftwareUpdates), CurrentState.HasLoggedInPermission(Permission.ViewDynamicUpdates), CurrentState.GetSupportAccountId().HasValue, CurrentState.HasLoggedInPermission(Permission.ViewCaseManagement),
		    //        //canSeeAutoFocusLink, CurrentState.HasLoggedInPermission(Permission.ViewASCDashboardReport), hasReturnsPortalAccess, CurrentState.HasLoggedInPermission(Permission.ViewPartnerServices), User.UserAccount.Id,supportAccountIdVal.Value);
		    //    //jumpToItems.AddRange(resourceLinks.Select(c => new SelectListItem { Text = c.Name, Value = c.Url}).ToArray());
		    //    return new SelectList(jumpToItems, "Value", "Text");
		    //}
		//}
	}

}