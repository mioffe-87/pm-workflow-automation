using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Web.Http.Results;
using System.Web.Mvc;
using Csp.Api.Model;
using Csp.Api.Services.Contracts;
using Csp.Web.DataTransferObjects;
using Csp.Web.HelperClasses;

namespace Csp.Web.Controllers
{
	[Authorize]
	public class SsoConfigurationController : BaseController
	{
		private readonly ISecurityLogic _securityLogic;

		public SsoConfigurationController(ISecurityLogic securityLogic)
		{
			_securityLogic = securityLogic;
		}

		[HttpGet]
		public ActionResult IdpConfiguration(long? id)
		{
			if (!id.HasValue || !_securityLogic.HasRole(id.Value, User.UserAccount.Id,RoleNames.DomainAdministrator)) {
				return RedirectToAction("UnAuthorizedAccess", "Error");
			}

			var model = new SsoConfigurationDto();

			try {
				var thirdPartySsoConfigUrl = string.Format(ConfigurationManager.AppSettings["ThirdPartySsoConfigUrl"], CurrentState.GetSupportAccountId());
				var email = User.UserAccount.Email;
				var domain = new MailAddress(email).Host;

				model.ThirdPartySsoConfigUrl = thirdPartySsoConfigUrl;
				model.Domain = domain;
			} catch (Exception ex) {
				Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
			}

			return View("IdpConfiguration", model);
			
		}
	}
}
