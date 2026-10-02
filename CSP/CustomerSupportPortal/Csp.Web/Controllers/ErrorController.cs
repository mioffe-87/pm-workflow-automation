using System.Web.Mvc;
using Csp.Web.DataTransferObjects;
using Csp.Web.HelperClasses;

namespace Csp.Web.Controllers {
	public class ErrorController : Controller {
		

		public ActionResult Error(string message) {

			return View("_Error");
		}

		public ActionResult PartialError(string message) {

			return PartialView("_Error");
		}

		public ViewResult NotFound() {
			Response.StatusCode = 404;  //you may want to set this to 200
			return View("_Error");
		}

		public ActionResult UnAuthorizedAccess(bool hasExpired = false, bool isPending = false, bool isValid = true)
		{
		    var model = new UnAuthorizedAccessDto();

            var userInfo = UserHelper.GetSsoToken();
		    if (userInfo != null)
		    {
                var userEmail = (string)userInfo["email"];
		        model.Email = userEmail;
		        model.HasExpiredMembership = false;
		        //var isImpersonated = (string)userInfo["isimpersonated"];
		        //if (userInfo["block"] != null)
		        //{
		        //    var isblocked = (userInfo["block"].ToString() == "1");

		        //    if (isblocked)
		        //    {
		        //        var test = "test";
		        //    }
		        //}
		    }
            model.HasExpiredMembership = hasExpired;
            model.IsMembershipPending = isPending;
            model.HasAccess = isValid;
            return View(model);
		}

        public ActionResult UnAuthorized()
        {
            return View();
        }

        public ActionResult SessionExpired()
        {
            return View();
        }

        public ActionResult UnAuthorizedUser(bool hasExpired = true, bool isPending = false, bool isValid = true)
        {
            string userEmail = "";
            var userInfo = UserHelper.GetSsoToken();
            if (userInfo != null)
            {
                userEmail = (string)userInfo["email"];
            }

            var model = new UnAuthorizedAccessDto()
            {
                HasExpiredMembership = hasExpired,
                IsMembershipPending = isPending,
                Email = userEmail,
                HasAccess = isValid
            };
            return View("UnAuthorizedAccess", model);
        }

        public ActionResult UnAuthorizedAccount()
        {
            var model = new UnAuthorizedAccessDto()
            {
                HasImpersonateAccessToAccount = false
            };
            return View("UnAuthorizedAccount", model);
        }
    }
}
