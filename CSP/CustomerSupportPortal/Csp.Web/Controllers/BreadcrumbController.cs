using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Csp.Api.Model;
using Csp.Web.HelperClasses;
using Csp.Api.Services.Contracts;
namespace Csp.Web.Controllers {
	public class BreadcrumbController : BaseController {



		public ActionResult Index(string tabTitle) {
            // Comments By LK on 08.26
            // Remove CurrentState Dependencies
			/*if (tabTitle == null || tabTitle.Trim().Equals(""))
				tabTitle = CurrentState.GetCurrentTabTitle();
			else
				CurrentState.SetCurrentTabTitle(tabTitle);*/

			return PartialView("../Shared/_Breadcrumb", BuildBreadcrumbList(tabTitle));
		}

		private List<Breadcrumb> BuildBreadcrumbList(string tabTitle) {

			List<Breadcrumb> builtBreadcrumbList = new List<Breadcrumb>();

			// Add the breadcrumbs from the Currentstate from the order of access:
			// Home, LoggedInAccount, CustomerAccount, Group, TabTitle

			// Add Home breadcrumb
			var wwwSupportOverviewUrl = System.Configuration.ConfigurationManager.AppSettings["WwwPanSupportUrl"];
			builtBreadcrumbList.Add(AddBreadcrumb("Support", wwwSupportOverviewUrl, true));

			// Add the Customer SupportAccount (if available)
			SupportAccount customerAccount = CurrentState.GetSupportAccount();
			if (customerAccount != null) {
				builtBreadcrumbList.Add(AddBreadcrumb(customerAccount.Name,
																		"/" + ControllerFields.SupportAccountController + "/MyAccounts/" + customerAccount.Id));
			}

			// Add the tabs title 
			builtBreadcrumbList.Add(AddBreadcrumb(tabTitle, null));

			return builtBreadcrumbList;
		}

		private Breadcrumb AddBreadcrumb(string title, string url, bool isRoot = false) {

			Breadcrumb aBreadcrumb = new Breadcrumb();
			aBreadcrumb.Title = title;
			aBreadcrumb.Url = url;
			aBreadcrumb.IsRoot = isRoot;
			return aBreadcrumb;
		}
	}
}


