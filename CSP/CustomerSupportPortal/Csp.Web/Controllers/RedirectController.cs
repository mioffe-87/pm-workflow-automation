using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using AutoMapper;
using Csp.Api.Services.Contracts;
using Csp.Web.HelperClasses;

namespace Csp.Web.Controllers {
	[Authorize]
	public class RedirectController : BaseController {

		private readonly IResourceLinkRepository _resourceLinkRepository;
		private readonly IHomeLogic _homeLogic;

		public RedirectController(IResourceLinkRepository resourceLinkRepository, IHomeLogic homeLogic) {
			_resourceLinkRepository = resourceLinkRepository;
			_homeLogic = homeLogic;
		}

		public ActionResult IndexPhp(string option = null, string task = null, string itemid = null) {

			var currentSupportAccountId = CurrentState.GetSupportAccountId();

			if (!currentSupportAccountId.HasValue) {
				//Try to guess the support account id
				currentSupportAccountId = _homeLogic.GetOrCreateDefaultSupportAccountId(User.UserAccount.Id);
			}

			if (option == "com_pan" && task == "viewcases" && itemid == "100") {

				if (!currentSupportAccountId.HasValue) {
					return RedirectToAction("MyAccounts", "SupportAccount");
				}

				var links = Mapper.Map<IEnumerable<SelectListItem>>(_resourceLinkRepository.GetResourceLinks()).ToList();
				var caseMgmtLink = links.Single(x => x.Text == "Case Management");
				return Redirect(caseMgmtLink.Value);
			}

			if (option == "com_pan" && task == "sw_updates" && itemid == "135") {
				return currentSupportAccountId.HasValue
					? RedirectToAction("SoftwareUpdates", "Updates", new { id = currentSupportAccountId })
					: RedirectToAction("MyAccounts", "SupportAccount");
			}

			return RedirectToAction("MyAccounts", "SupportAccount");
		}
	}
}
