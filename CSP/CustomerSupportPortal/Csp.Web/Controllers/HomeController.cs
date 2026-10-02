using System.Web.Security;
using System;
using System.Web;
using System.Web.Mvc;
using Csp.Api.Logic;
using Csp.Api.Model;
using Csp.Web.HelperClasses;
using Csp.Api.Services.Contracts;
using System.Configuration;
using Csp.Api.Model.Enums;
using Csp.Api.HelperClasses;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace Csp.Web.Controllers {

	public class HomeController : BaseController {
        private readonly IHomeLogic _homeLogic;
        private readonly ILoginHistoryLogic _loginHistoryLogic;
        private readonly IImpersonateRequestLogic _impersonateRequestLogic;
        private readonly ISupportAccountUserLogic _supportAccountUserLogic;
        private readonly string impersonate_topic = ConfigurationManager.AppSettings.Get("Impersonate_TopicName");
        private readonly string impersonate_project = ConfigurationManager.AppSettings.Get("Impersonate_ProjectName");
        private readonly string _loginUrl = ConfigurationManager.AppSettings["LoginUrl"];

        public HomeController(IHomeLogic homeLogic, ILoginHistoryLogic loginHistoryLogic, IImpersonateRequestLogic impersonateRequestLogic, ISupportAccountUserLogic supportAccountUserLogic)
        {
            _homeLogic = homeLogic;
            _loginHistoryLogic = loginHistoryLogic;
            _impersonateRequestLogic = impersonateRequestLogic;
            _supportAccountUserLogic = supportAccountUserLogic;
        }

		public ActionResult Index() {
            return RedirectToActionPermanent("Index", "Support");
        }

		public ActionResult NotAuthorized() {
			return View("_NotAuthorized");
		}

        public ActionResult Logout()
        {
            string returnUrl = ConfigurationManager.AppSettings["LoginServiceLogoutUrl"];
            
            try
            {
                HttpCookie httpCookie = Request.Cookies[ApiConstants.Impersonate_Session_Identifier];
                if (httpCookie != null)
                {
                   string sessionId = httpCookie.Value;

                   if (!string.IsNullOrEmpty(sessionId))
                   {
                       // Update login history
                       var loginHistory = new LoginHistoryModel()
                       {
                           SessionId = sessionId,
                           UserAccountId = User.UserAccount.Id,
                           SupportAccountId = CurrentState.GetSupportAccountId()
                       };

                       _loginHistoryLogic.UpdateLoginHistory(loginHistory);

                       var loginDetails = _loginHistoryLogic.GetLoginHistory(Session.SessionID);
                       if (!string.IsNullOrEmpty(loginDetails.ImpersonateRequestId))
                       {
                           loginDetails.LogoutTimeAsString = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                            EventPublisher eventPublisher = new EventPublisher();
                            eventPublisher.PublishCustomEvent(loginDetails, impersonate_project, impersonate_topic);
                       }
                    }
                   
                }

                HttpCookie historyCookie = Request.Cookies[ApiConstants.Login_User_Session_Identifier];
                if (historyCookie != null)
                {
                    string sessionId = historyCookie.Value;
                    if (!string.IsNullOrEmpty(sessionId))
                    {
                        // Update login history
                        var loginUserHistory = new LoginHistoryModel()
                        {
                            SessionId = sessionId,
                            UserAccountId = User.UserAccount.Id,
                            SupportAccountId = CurrentState.GetSupportAccountId()
                        };

                        _loginHistoryLogic.UpdateLoginUserHistory(loginUserHistory);
                    }
                }

            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            // Invalidate JWT tokens in csp-auth
            _supportAccountUserLogic.InvalidateJwtTokens(User.UserAccount.Id);

            // Must do this after the GetIsImpersonated check above as this will clear the value
            CurrentState.ClearAll();
            Session.Clear();
            Session.Abandon();
            FormsAuthentication.SignOut();
            ClearLoginCookie();
            ClearImpersonation();
            ClearJwtSession();

            ViewBag.ReturnUrl = returnUrl;
            return View("SsoLogout");
        }

        [Authorize]
		public ActionResult Impersonate(string requestId) {
            if (string.IsNullOrWhiteSpace(requestId) 
                || !Regex.IsMatch(requestId, @"^[a-zA-Z0-9\-]+$"))
            {
                HttpException ex = new HttpException(404, "Parameter 'requestId' is required, must contain only alhanumeric characters or '-', no spaces.");
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw ex;
            }

            //get the target user
            dynamic result = _impersonateRequestLogic.ProcessRequest(new ImpersonateRequest()
            {
                ImpersonateAction = ImpersonateAction.Get.ToDescription(),
                ImpersonateRequestId = requestId,
                RequestorUserAccountId = User.UserAccount.Id
            });

            if (result != null)
            {
                ActiveRequests impersonateDetails = JsonConvert.DeserializeObject<ActiveRequests>(JsonConvert.SerializeObject(result.ResponseObject));
                string targetEmail = impersonateDetails.targetUserAccount;

                //get the current user (csr) userid from enterprise okta
                // Update the CSRs username to the customer's email
                bool isSuccess = _impersonateRequestLogic.UpdateDataForImpersonate(User.UserAccount.Email, targetEmail);

                if (isSuccess)
                {
                    // clear sessions
                    CurrentState.ClearAll();
                    Session.Clear();
                    Session.Abandon();
                    FormsAuthentication.SignOut();
                    ClearLoginCookie();
                    ClearJwtSession();
                    //Form impersonate urls
                    string encryptedDetails = SecurityHelper.ConvertStringToHex(SecurityHelper.Encrypt(string.Concat(User.UserAccount.Email, "|", impersonateDetails.targetSupportAccountId)));
                    string relayStateUrl = HttpUtility.UrlEncode(string.Concat(_loginUrl, "?RelayState=/SupportAccount/MyAccount?impersonateData=", encryptedDetails));
                    string redirectUrl = string.Format("{0}{1}", ConfigurationManager.AppSettings["ImpersonateUrl"], relayStateUrl);
                    return Redirect(redirectUrl);
                }
            }
            return RedirectToAction("MyAccounts", "SupportAccount");
        }

	    public ActionResult Header()
	    {
	        return View();
	    }
        
        [Authorize]
        [HttpGet]
        public ActionResult AccountSelector(long? id) {           

            return View();
        }

        public void ClearImpersonation()
        {
            //clear impersonate data cookie
            HttpCookie currentUserCookie = Request.Cookies[ApiConstants.Impersonation_Identifier];
            if (currentUserCookie != null)
            {
                Response.Cookies.Remove(ApiConstants.Impersonation_Identifier);
                currentUserCookie.Expires = DateTime.Now.AddDays(-1);
                currentUserCookie.Value = null;
                Response.SetCookie(currentUserCookie);
            }

            //clear impersonate session id cookie
            HttpCookie impersonateSessionCookie = Request.Cookies[ApiConstants.Impersonate_Session_Identifier];
            if (impersonateSessionCookie != null)
            {
                Response.Cookies.Remove(ApiConstants.Impersonate_Session_Identifier);
                impersonateSessionCookie.Expires = DateTime.Now.AddDays(-1);
                impersonateSessionCookie.Value = null;
                Response.SetCookie(impersonateSessionCookie);
            }
        }

        private void ClearLoginCookie()
        {
            HttpCookie loginCookie = Request.Cookies[ApiConstants.Login_Identifier];
            if (loginCookie != null)
            {
                Response.Cookies.Remove(ApiConstants.Login_Identifier);
                loginCookie.Expires = DateTime.Now.AddDays(-1);
                loginCookie.Value = null;
                Response.SetCookie(loginCookie);
            }

            HttpCookie historyCookie = Request.Cookies[ApiConstants.Login_User_Session_Identifier];
            if (historyCookie != null)
            {
                Response.Cookies.Remove(ApiConstants.Login_User_Session_Identifier);
                historyCookie.Expires = DateTime.Now.AddDays(-1);
                historyCookie.Value = null;
                Response.SetCookie(historyCookie);
            }
        }

        private void ClearJwtSession()
        {
            HttpCookie supportSessionCookie = Request.Cookies[ApiConstants.SupportSessionId];
            if (supportSessionCookie != null)
            {
                Response.Cookies.Remove(ApiConstants.SupportSessionId);
                supportSessionCookie.Expires = DateTime.Now.AddDays(-1);
                supportSessionCookie.Value = null;
                Response.SetCookie(supportSessionCookie);
            }

            HttpCookie cspRefreshTokenCookie = Request.Cookies[ApiConstants.CspRefreshToken];
            if (cspRefreshTokenCookie != null)
            {
                Response.Cookies.Remove(ApiConstants.CspRefreshToken);
                cspRefreshTokenCookie.Expires = DateTime.Now.AddDays(-1);
                cspRefreshTokenCookie.Value = null;
                Response.SetCookie(cspRefreshTokenCookie);
            }
        }

        public ActionResult Register(string code, string message, string history)
        {
            //string decryptedTenantId = SecurityHelper.Decrypt(SecurityHelper.ConvertHexToString(_ptx));
            return View();
        }

        public ActionResult LoginHelp()
        {
            return RedirectToActionPermanent("LoginHelp", "Support");
        }
    }
}
