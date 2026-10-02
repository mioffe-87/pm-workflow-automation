using System.Configuration;
using System.Text;
using System.Web.Mvc;
using Csp.Api.Model.GoogleApis;
using Csp.Api.Security;
using Csp.Web.Attributes;
using Csp.Web.Security.Attributes;

namespace Csp.Web.Controllers
{
    public class GoogleOauthController : BaseController
    {
        /*
        public ActionResult OauthCallback(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return PartialView("../Partial/_GoogleOauthCallback", null);
            }

            var userInfo = new GoogleUserInfo
            {
                Email = User.UserAccount.Email,
                Name = code
            };

            return PartialView("../Partial/_GoogleOauthCallback", userInfo);
        }

        [HttpGet, AjaxOnly, CspAuthorize(Permission.RegisterDevice)]
        public ActionResult GetOauthUrl()
        {
            var redirectUrl = ConfigurationManager.AppSettings.Get("GoogleOauthRedirectUrl");
            var clientId = ConfigurationManager.AppSettings.Get("GoogleOauthClientId");
            var url = string.Format(
                "https://accounts.google.com/o/oauth2/v2/auth?redirect_uri={0}&prompt=consent&response_type=code&client_id={1}&scope=https://www.googleapis.com/auth/cloud-billing-partner-subscriptions.readonly+https://www.googleapis.com/auth/userinfo.profile&access_type=offline",
                redirectUrl, clientId);
            var encodedBytes = Encoding.UTF8.GetBytes(url);
            var result = new
            {
                GoogleOauthUrl = System.Convert.ToBase64String(encodedBytes)
            };
            return Json(result, JsonRequestBehavior.AllowGet);
        }
         */
    }
}
