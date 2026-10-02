using System;
using System.Web.Http;
using System.Collections;
using System.IO;
using System.Web;
using System.Text;
using System.Configuration;
using opentoken;
using Csp.Web.Filters;
using Csp.Api.Services.Contracts;
using Csp.Api.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Csp.Api.HelperClasses;
using Csp.Api.Logic;
using Csp.Web.HelperClasses;
using System.Web.Caching;
using Base32;
using Csp.Api.Model.Enums;
using Csp.Web.Attributes;
using Microsoft.Security.Application;
using OtpSharp;

namespace Csp.Web.Controllers
{
    public class UserProfileController : ApiController
    {
        private readonly IUserProfileDataAccess _userProfileDataAccess;
        private readonly ISalesforceRestApiLogic _salesforceRestApiLogic;
        private readonly ISupportAccountLogic _supportAccountLogic;
        private readonly IMfaLogic _mfaLogic;
        private readonly ISecurityLogic _securityLogic;
        private readonly IUserAccountLogic _userAccountLogic;
        private readonly IConsentRequestLogic _consentRequestLogic;
        private readonly ISupportAccountUserLogic _supportAccountUserLogic;

        public UserProfileController(IUserProfileDataAccess userProfileDataAccess, ISecurityLogic securityLogic, ISalesforceRestApiLogic salesforceRestApiLogic, ISupportAccountLogic supportAccountLogic, IMfaLogic mfaLogic, IUserAccountLogic userAccountLogic,
            IConsentRequestLogic consentRequestLogic, ISupportAccountUserLogic supportAccountUserLogic)
        {
            _userProfileDataAccess = userProfileDataAccess;
            _salesforceRestApiLogic = salesforceRestApiLogic;
            _supportAccountLogic = supportAccountLogic;
            _mfaLogic = mfaLogic;
            _securityLogic = securityLogic;
            _userAccountLogic = userAccountLogic;
            _consentRequestLogic = consentRequestLogic;
            _supportAccountUserLogic = supportAccountUserLogic;
        }

        // GET: api/UserProfile/{email}
        [HttpGet]
        [JwtTokenAuthentication]
        public IHttpActionResult GetUserData(string email)
        {
            // Fix for CSP-4083 - Remove GetSafeHtmlFragment as the email address is already decoded
            // email = Sanitizer.GetSafeHtmlFragment(email);
            if (!JwtTokenManager.IsAuthorized(email))
            {
                return Unauthorized();
            }

            //Fix for CSP-707 - Removed double decoding of email as it is already decoded by ASP.NET
	        //  email= WebUtility.UrlDecode(email);
            var returnData = new UserProfileModel();
            try
            {
                returnData = _userProfileDataAccess.GetUserData(email);
                returnData.IsMfaRequired = _mfaLogic.IsMfaRequiredForUser(email);
                returnData.IsFedramp = _userProfileDataAccess.IsFedrampUser(email);
                returnData.IsThirdPartyIdpConfigured = SiteHelper.IsThirdPartyIdpConfigured(email) && !_securityLogic.HasRole(0,Convert.ToInt64(returnData.UserAccountId),RoleNames.DomainAdministrator);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return this.Json(returnData);
        }

        // GET: api/UserProfile/{email}
        [HttpGet]
        [JwtTokenAuthentication]
        public IHttpActionResult GetUserProfile(string email)
        {
            // Fix for CSP-4083 - Remove GetSafeHtmlFragment as the email address is already decoded
            // email = Sanitizer.GetSafeHtmlFragment(email);
            if (!JwtTokenManager.IsAuthorized(email))
            {
                return Unauthorized();
            }

            //Fix for CSP-707 - Removed double decoding of email as it is already decoded by ASP.NET
	        //  email = WebUtility.UrlDecode(email);
            var userProfile = new UserProfileModel();

            try
            {
                userProfile = _userProfileDataAccess.GetUserData(email);

            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return this.Json(userProfile);
        }

        // GET: api/UserProfile/GetAllCountries
        //[HttpGet]
        //public IHttpActionResult GetAllCountries(bool isPartialShipment=false)
        //{
        //    var returnData = new List<countries>();
        //    try
        //    {
        //        returnData = _userProfileDataAccess.GetCountries(isPartialShipment);
        //    }
        //    catch (Exception ex)
        //    {
        //        Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
        //    }

        //    return this.Json(returnData);
        //}

        [HttpGet]
        public IHttpActionResult GetRegions()
        {
            var returnData = new List<regions>();
            try
            {
                returnData = _userProfileDataAccess.GetRegions();
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return this.Json(returnData);
        }

        // GET: api/UserProfile/GetRegionsByCountryId/1
        //[HttpGet]
        //public IHttpActionResult GetRegions(int countryId)
        //{
        //    //var returnData = new List<regions>();
        //    try
        //    {
        //        //returnData = _userProfileDataAccess.GetRegions(countryId);
        //        var result = _supportAccountUserLogic.GetRegions(countryId);
        //        return this.Json(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
        //    }

        //    return this.Json(string.Empty);
        //}

        // POST: api/UserProfile
        [HttpPost, HttpOptions]
        [JwtTokenAuthentication]
        public IHttpActionResult UpdateUserData([FromBody] UserProfileModel userData)
        {
	        if (!JwtTokenManager.IsAuthorized(userData.Email))
	        {
		        return Unauthorized();
	        }

	        var returnData = new Resultdetails();

	        try
	        {
                // Comments By LK On 08/02
                // Implement fix for XSS attacks
                //Use HtmlDecode to handle all HTML entities in email
                string sanitizedEmail = Sanitizer.GetSafeHtmlFragment(userData.Email);
                sanitizedEmail = System.Web.HttpUtility.HtmlDecode(sanitizedEmail);

                string sanitizedPhoneNumber = Sanitizer.GetSafeHtmlFragment(userData.PhoneNumber);
                sanitizedPhoneNumber = System.Web.HttpUtility.HtmlDecode(sanitizedPhoneNumber);

                UserDetail userDetail = new UserDetail()
                {
                    Email = sanitizedEmail,
                    FirstName = Sanitizer.GetSafeHtmlFragment(userData.FirstName),
                    LastName = Sanitizer.GetSafeHtmlFragment(userData.LastName),
                    DisplayName = Sanitizer.GetSafeHtmlFragment(userData.DisplayName),
                    Title = Sanitizer.GetSafeHtmlFragment(userData.Title),
                    Address1 = Sanitizer.GetSafeHtmlFragment(userData.Address1),
                    Address2 = Sanitizer.GetSafeHtmlFragment(userData.Address2),
                    City = Sanitizer.GetSafeHtmlFragment(userData.City),
                    Region = Sanitizer.GetSafeHtmlFragment(userData.Region),
                    PostalCode = Sanitizer.GetSafeHtmlFragment(userData.PostalCode),
                    PhoneNumber = sanitizedPhoneNumber,
                    RegisteredAppId = ApiConstants.CspRegisterAppId,
                    CountryId = userData.Country,
                    Comments = Sanitizer.GetSafeHtmlFragment(userData.Comments)
                };

               var result = _supportAccountUserLogic.UpdateUser(userDetail);
                return this.Json(result);
            }
	        catch (Exception ex)
	        {
		        Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
	        }

            return this.Json(string.Empty);
        }

        [HttpGet]
        [JwtTokenAuthentication]
        public IHttpActionResult IsDisplayNameExists(string displayName, string userEmail)
        {
            if (!JwtTokenManager.IsAuthorized(userEmail))
            {
                return Unauthorized();
            }

            //userEmail = WebUtility.UrlDecode(userEmail);
            var returnData = new Resultdetails();

            try
            {
                //check if displayname already exists
                bool isAlreadyExists = _userProfileDataAccess.IsDisplayNameExists(displayName, userEmail);
                returnData.isSuccess = isAlreadyExists;

            }
            catch (Exception ex)
            {
                //Add logging to elmah here
                returnData.isSuccess = false;
                returnData.message = "Failed to update check if Display Name exists.Please contact Support.";

                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return this.Json(returnData);
        }

        [AllowAnonymous]
        [HttpGet]
        public IHttpActionResult GetEmail()
        {
            IDictionary userInfo = null;
            // Comments By LK On 02/06
            // TODO: Tech debt need to move UserDetails to Csp.Api.Model and follow the Pascal Case naming convention

	          var userData = new UserProfileModel();
            userdetails userdetail = new userdetails();

						//var salesforceCookie = HttpContext.Current.Request.Cookies[Constants.SALESFORCE_COOKIE];
						//var idptokenCookie = HttpContext.Current.Request.Cookies[Constants.SESSION_COOKIE];

            using (var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(CorrectSsoValue())))
            {
                var agent = new Agent(memoryStream);

                try
                {
                    userInfo = agent.ReadToken(HttpContext.Current.Request);

                }
                catch (Exception ex)
                {
                    Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                    return this.Json(userdetail);
                }
            }
	          //if support token exists
            if (userInfo != null)
            {

                userdetail.email = (string)userInfo["email"];
                userdetail.fullName = string.Concat(userInfo["firstname"], " ", userInfo["lastname"]);
                userdetail.isLoggedin = userdetail.email.Length > 0 ? true : false;
                userdetail.canImpersonate = userdetail.email.Contains("@paloaltonetworks.com");
            }

	          //if SF is accessed directly
						//else if (salesforceCookie != null) {
						//	userData = _userProfileDataAccess.GetUserData(Convert.ToString(salesforceCookie.Value));

						//	if (userData != null) {
						//		userdetail.email = userData.Email;
						//		userdetail.fullName = string.Concat(userData.FirstName, " ", userData.LastName);
						//		userdetail.isLoggedin = userData.Email.Length > 0 ? true : false;
						//		userdetail.canImpersonate = userData.Email.Contains("@paloaltonetworks.com");
						//	}
						//}


            return this.Json(userdetail);
        }

        [HttpGet]
        [JwtTokenAuthentication]
        public async Task<IHttpActionResult> GetCaseAlerts(string email, long accountId)
        {
            if (!JwtTokenManager.IsAuthorized(email))
            {
                return Unauthorized();
            }

            //Fix for CSP-707 - Removed double decoding of email as it is already decoded by ASP.NET
	        //email = WebUtility.UrlDecode(email);

            var caseAlertsList = new List<SupportCaseInfo>();
            var caseAlerts = new List<AlertInfo>();
            var rmaAlerts = new List<AlertInfo>();

            var alertGroups=new List<AlertGroups>();
            var notifications=new Notifications();

            var salesforceCaseDetailUrl = ConfigurationManager.AppSettings["SalesforceCaseDetailUrl"];

            try
            {
                DateTime userLastLoginDateTime = DateTime.MinValue;
                var userProfile = _userProfileDataAccess.GetUserData(email);
                var userAccountId = userProfile.UserAccountId.HasValue ? userProfile.UserAccountId.Value : 0;
                var supportAccount = _supportAccountLogic.GetSupportAccount(accountId, userAccountId);
                var hasCaseManagementAccess = _securityLogic.HasViewCaseManagementPrivilege(accountId, userAccountId);

                if (accountId == ApiConstants.PANW_SUPPORT_ACCOUNT_ID || email.Contains(ApiConstants.PANW_EMPLOYEE_EMAIL_EXTENSION))
                {
                    userLastLoginDateTime = await _salesforceRestApiLogic.GetUserLastLoginDateTime(userAccountId, accountId);
                }
                else
                {
                    userLastLoginDateTime = _userProfileDataAccess.GetUserLastLoginDateTime(email);
                }

                if (!hasCaseManagementAccess)
                {
                    return this.Json(caseAlertsList);
                }

                if (userLastLoginDateTime != DateTime.MinValue)
                {
                    caseAlertsList = await _salesforceRestApiLogic.GetCaseAlerts(supportAccount.SalesforceId, userLastLoginDateTime);
                }

                foreach (var item in caseAlertsList)
                {
                    var feedInfo = new AlertInfo();
                    var caseDetailUrl = string.Format(salesforceCaseDetailUrl, item.Id);

                    feedInfo.Title = item.CaseNumber;

                    feedInfo.URL = item.IsRma
                        ? string.Format("{0}%26selectRMATab={1}", caseDetailUrl,
                            item.IsRma.ToString().ToLower())
                        : caseDetailUrl;

                    switch (item.Status)
                    {
                        case "Awaiting Customer Verification":
                            feedInfo.Summary = string.Format("Support case {0} is awaiting customer verification", item.CaseNumber);
                            break;
                        case "Customer Action Requested":
                            feedInfo.Summary = string.Format("Customer action requested for support case {0}", item.CaseNumber);
                            break;
                        case "Pending Customer":
                            feedInfo.Summary = string.Format("Case {0} is pending customer's input", item.CaseNumber);
                            break;
                    }

                    if(item.IsRma)
                        rmaAlerts.Add(feedInfo);
                    else
                        caseAlerts.Add(feedInfo);
                   
                }

               if (rmaAlerts.Any())
                {
                    var rmaGroup = new AlertGroups()
                    {
                        Name = "RMA",
                        Items = rmaAlerts,
                        IconClass = ApiConstants.RMA_ALERTS_ICONCLASS.ToString()
                    };

                    alertGroups.Add(rmaGroup);
                   
                }
               

                if (caseAlerts.Any())
                {
                    var caseGroup = new AlertGroups()
                    {
                        Name = "CASE UPDATES",
                        Items = caseAlerts,
                        IconClass = ApiConstants.CASE_ALERTS_ICONCLASS.ToString()
                    };

                    alertGroups.Add(caseGroup);
                }

                if (alertGroups.Any())
                {
                    notifications = new Notifications()
                    {
                        Groups = alertGroups,
                        Count = caseAlerts.Count + rmaAlerts.Count
                    };


                }

            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return this.Json(notifications);
        }

        [HttpPost, HttpOptions]
        public IHttpActionResult IsAscUser([FromBody] UserDetailModel model)
        {
            bool isAscUser = false;

            try
            {
                var privateKey = ConfigurationManager.AppSettings["EncryptionProviderPrivateKey"];
                var encryptionProvider = new EncryptionProvider();

                var isKeyValid = encryptionProvider.IsKeyValid(model.Email, model.Key, privateKey);
                if (!isKeyValid)
                {
                    Elmah.ErrorSignal.FromCurrentContext()
                        .Raise(new Exception(String.Format("Invalid key {0}", model.Key)));
                    return this.Json(new PortalContextDetails());
                }

                isAscUser = !string.IsNullOrEmpty(model.Username) && _userProfileDataAccess.IsAscUserBySfdcLoginId(model.Username);
            }
            catch (Exception ex)
            {
                var errorMsg = "Error in call to IsASCUser()";

                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(errorMsg));
            }

            return this.Json(isAscUser);
        }

        private static string CorrectSsoValue()
        {
            return ConfigurationManager.AppSettings["AgentConfigValue"];
        }

        private static bool IsLoggedIn(HttpRequest request)
        {
            var isLoggedIn = false;
            IDictionary userInfo = null;

            using (var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(CorrectSsoValue())))
            {
                var agent = new Agent(memoryStream);

                try
                {
                    userInfo = agent.ReadToken(request);

                }
                catch (Exception ex)
                {
                    Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                }
            }

            isLoggedIn = (userInfo != null && userInfo["email"].ToString().Length > 0);

            return isLoggedIn;
        }
	   

	    [HttpGet]
	    [JwtTokenAuthentication]
        public IHttpActionResult GetQuickActionLinks(string email, string supportaccountid) {
            
            if (!JwtTokenManager.IsAuthorized(email))
            {
                return Unauthorized();
            }

            var quickActionLinks = new List<QuickActionLinkItem>();

            try {
                var cacheKey = $"{email}_{supportaccountid}_QuickActionLinks";
                var cacheData = HttpRuntime.Cache.Get(cacheKey);

                if (cacheData == null) {
                    var quickLinks = _userProfileDataAccess.GetQuickActionLinks(email, supportaccountid);

                    quickActionLinks = quickLinks.ToList();
                    HttpRuntime.Cache.Insert(cacheKey, quickLinks.ToList(), null, DateTime.Now.AddMinutes(5), Cache.NoSlidingExpiration);
                }
                else {
                    quickActionLinks = (List<QuickActionLinkItem>)cacheData;
                }
            } catch (Exception ex) {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return Json(quickActionLinks);
        }


	    [HttpGet]
	    [JwtTokenAuthentication]
        [Obsolete]
	    public IHttpActionResult GetUserSubscriptions(string email)
        {
            if (!JwtTokenManager.IsAuthorized(email))
            {
                return Unauthorized();
            }

            var emailSubscriptions = new EmailSubscriptions();

            //GetUserSubscriptions
            //Fix for CSP-707 - Removed double decoding of email as it is already decoded by ASP.NET
            //email = WebUtility.UrlDecode(email);
            try
            {
                var userProfile = _supportAccountUserLogic.GetUserData(email);
                if (userProfile != null && userProfile.UserAccountId.HasValue)
                {
                    emailSubscriptions = _supportAccountUserLogic.UserSubscriptions(userProfile.UserAccountId.Value, ManageUserAction.Get.ToDescription());
                }                
		    }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
		    }
            return this.Json(emailSubscriptions);
        }

	    [HttpPost]
        [JwtTokenAuthentication]
        [Obsolete]
        public IHttpActionResult UpdateEmailSubscriptions(EmailSubscriptions emailSubscriptions)
        {
            if (!JwtTokenManager.IsAuthorized(emailSubscriptions.email))
            {
                return Unauthorized();
            }

		    var returnData = new Resultdetails();

		    try
            {
                var userProfile = _supportAccountUserLogic.GetUserData(emailSubscriptions.email);
                emailSubscriptions.lastUpdateByUserAccountId = userProfile.UserAccountId;
                var response = _supportAccountUserLogic.UserSubscriptions(userProfile.UserAccountId.Value, ManageUserAction.Update.ToDescription(), emailSubscriptions);
                var updateStatus = response != null && response.emailSubscriptionId > 0 ? true : false;
                returnData.isSuccess = userProfile != null && userProfile.UserAccountId.HasValue ? updateStatus : false;

			    if (returnData.isSuccess)
                {
                    returnData.message = "Email Subscriptions updated succesfully";
			    }
                else
                {
				    returnData.isSuccess = false;
				    returnData.message = "Email Subscriptions failed to update";
			    }
		    }
            catch (Exception ex)
            {
			    //Add logging to elmah here
			    returnData.isSuccess = false;
			    returnData.message = "Failed to update email subscriptions. Please contact Support.";
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
		    return this.Json(returnData);
	    }


	    [HttpGet]
	    [JwtTokenAuthentication]
        [GzipCompression]
        public IHttpActionResult GetAvailableAccounts(string email) {
            if (!JwtTokenManager.IsAuthorized(email))
            {
                return Unauthorized();
            }

            var availableAccountsList = new List<NameValueOnly>();

            try
            {
                var cacheKey = $"{email}__AvailableAccounts";
                var cacheData = HttpRuntime.Cache.Get(cacheKey);

                if (cacheData == null)
                {
                    var user = _userAccountLogic.GetUserAccountByEmail(email);
                    var accounts = _securityLogic.GetAvailableAccounts(user.Id);
                    var defaultSupportAccountId = _userAccountLogic.GetDefaultSupportAccountByUser(user.Id);
                    var supportAccount = _supportAccountLogic.GetSupportAccount(defaultSupportAccountId, user.Id);

                    if (defaultSupportAccountId > 0)
                    {
                        availableAccountsList.Add(new NameValueOnly() { Name = string.Format("{0} (Default)", supportAccount.Name.TrimStart()), Value = supportAccount.Id.ToString() });
                        accounts.RemoveAll(a => a.SupportAccountId == defaultSupportAccountId);
                    }

                    var availableAccounts = accounts.Select(x => new NameValueOnly() { Name = x.CompanyName.TrimStart(), Value = x.SupportAccountId.ToString() }).ToList();
                    availableAccountsList.AddRange(availableAccounts);

                    HttpRuntime.Cache.Insert(cacheKey, availableAccountsList, null, DateTime.Now.AddMinutes(5), Cache.NoSlidingExpiration);
                }
                else
                {
                    availableAccountsList = (List<NameValueOnly>) cacheData;
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return Ok(availableAccountsList);
	    }

        [HttpGet]
        [JwtTokenAuthentication]
        public IHttpActionResult EnableGoogleAuthenticator(string email)
        {
            if (!JwtTokenManager.IsAuthorized(email))
            {
                return Unauthorized();
            }

            var secretKey = KeyGeneration.GenerateRandomKey(10);
            var issuer = HttpUtility.UrlEncode("Palo Alto Networks");
            var barcodeUrl = string.Format("{0}&issuer={1}", KeyUrl.GetTotpUrl(secretKey, email), issuer);

            var googleAuthModel = new GoogleAuthenticatorModel
            {
                UserEmail = email,
                SecretKey = Base32Encoder.Encode(secretKey),
                BarcodeUrl = barcodeUrl
            };

            return Json(googleAuthModel);
        }

        [HttpPost]
        [JwtTokenAuthentication]
        public IHttpActionResult EnableGoogleAuthenticator(GoogleAuthenticatorModel googleAuthenticatorModel)
        {
            if (!JwtTokenManager.IsAuthorized(googleAuthenticatorModel.UserEmail))
            {
                return Unauthorized();
            }

            var returnData = new Resultdetails();

            try
            {
                byte[] secretKey = Base32Encoder.Decode(googleAuthenticatorModel.SecretKey);
                var otp = new Totp(secretKey);
                long timeStepMatched = 0;
                
                if (otp.VerifyTotp(googleAuthenticatorModel.Code, out timeStepMatched, new VerificationWindow(2, 2)))
                {
                    var userModel = _userProfileDataAccess.GetUserData(googleAuthenticatorModel.UserEmail);
                    // Call Ping API to obfuscate the secret Key
                    var obfuscatedSecret = _mfaLogic.ObfuscateSecret(googleAuthenticatorModel.SecretKey); 
                    var user = new UserAccount { 
                        Id = userModel.UserAccountId ?? 0, 
                        FirstName = userModel.FirstName, 
                        LastName = userModel.LastName, 
                        Email = userModel.Email, 
                        IsMfaEnabled = userModel.IsMfaEnabled,
                        MultiFactorAuthType = userModel.MultiFactorAuthType,
                        IsGoogleAuthEnrolled = !string.IsNullOrEmpty(obfuscatedSecret)
                    };
                    
                    //Enable Google Authenticator for MFA
                    _mfaLogic.EnableMfa(user, user.Id, MultiFactorAuthType.GoogleAuthenticator,obfuscatedSecret);
                    returnData.isSuccess = true;
                    returnData.message = "User has been successfully enrolled in Google Authenticator";
                }
                else
                {
                    returnData.isSuccess = false;
                    returnData.message = "The code is not valid";
                }
            }
            catch (Exception ex)
            {
                //Add logging to elmah here
                returnData.isSuccess = false;
                returnData.message = "Failed to enable Google Authenticator. Please contact Support.";

                throw new Exception(ex.Message);
            }

            return Json(returnData);
        }

        [HttpGet]
        [JwtTokenAuthentication]
        public IHttpActionResult IsFirstLogin(string userEmail)
        {
            if (!JwtTokenManager.IsAuthorized(userEmail))
            {
                return Unauthorized();
            }

            var userConfig = new UserConfig();

            try
            {
                var cacheKey = $"{userEmail}_IsFirstLogin";
                var cacheData = HttpRuntime.Cache.Get(cacheKey);

                if (cacheData == null)
                {
                    userConfig = _userProfileDataAccess.IsFirstLogin(userEmail);
                    HttpRuntime.Cache.Insert(cacheKey, userConfig, null, DateTime.Now.AddHours(8), Cache.NoSlidingExpiration);
                }
                else
                {
                    userConfig = (UserConfig) cacheData;
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return this.Json(userConfig);
        }

        #region rewrite - start
        [JwtTokenAuthentication]
        [HttpPost]
        public IHttpActionResult ManageUsers([FromBody] SupportAccountUser requestData, long supportAccountId, long userAccountId, string action, bool extension = false)
        {
            long actionPerformedBy = userAccountId;

            if (ManageUserAction.Create.ToDescription().Equals(action,StringComparison.OrdinalIgnoreCase))
            {
                actionPerformedBy = requestData.CreatedByUserAccountId;
            }
            else if (ManageUserAction.Update.ToDescription().Equals(action, StringComparison.OrdinalIgnoreCase))
            {
                actionPerformedBy = requestData.LastUpdatedByUserAccountId;
            }


            if (!JwtTokenManager.IsAuthorized(actionPerformedBy, supportAccountId, ClaimType.UserAccountId))
            {
                return Unauthorized();
            }

            try
            {
                var result = _supportAccountUserLogic.SupportAccountUsers(supportAccountId, userAccountId, action.ToLower(), requestData, extension);
                return this.Json(result);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(string.Empty);
        }

        [JwtTokenAuthentication]
        [HttpGet]
        public IHttpActionResult GetUserPrivileges(long supportAccountId, long userAccountId)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId.ToString(), ClaimType.UserAccountId))
            {
                return Unauthorized();
            }
            try
            {
                var result = _supportAccountUserLogic.SupportAccountUserPrivileges(supportAccountId, userAccountId);
                return this.Json(result);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(string.Empty);
        }

        [JwtTokenAuthentication]
        [HttpGet]
        public IHttpActionResult GetAssignableRoles(long supportAccountId, long userAccountId)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId))
            {
                return Unauthorized();
            }
            try
            {
                var result = _supportAccountUserLogic.GetAssignableRoles(supportAccountId, userAccountId);
                return this.Json(result);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(string.Empty);
        }

        [JwtTokenAuthentication]
        [HttpGet]
        public IHttpActionResult GetSiteLicense(long supportAccountId, long userAccountId)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId))
            {
                return Unauthorized();
            }
            try
            {
                var result = _supportAccountUserLogic.GetSiteLicense(supportAccountId);
                return this.Json(result);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(string.Empty);
        }

        [JwtTokenAuthentication]
        [HttpGet]
        public IHttpActionResult GetDefaultAddress(long supportAccountId, long userAccountId)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId))
            {
                return Unauthorized();
            }
            try
            {
                var result = _supportAccountUserLogic.GetSupportAccountDetails(supportAccountId, false);
                return this.Json(result);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(string.Empty);
        }


        // GET: api/UserProfile/GetAllCountries
        [HttpGet]
        public IHttpActionResult GetAllCountries(bool isPartialShipment = false)
        {
            try
            {
                var result = _supportAccountUserLogic.GetCountries();
                return this.Json(result);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return this.Json(string.Empty);
        }

        // GET: api/UserProfile/GetRegionsByCountryId/1
        [HttpGet]
        public IHttpActionResult GetRegions(int countryId)
        {
            try
            {
                var result = _supportAccountUserLogic.GetRegions(countryId);
                return this.Json(result);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(string.Empty);
        }

        [JwtTokenAuthentication]
        [HttpPost]
        public IHttpActionResult CreateUser([FromBody] UserDetail requestData, long userAccountId)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, requestData.DefaultSupportAccountId, ClaimType.UserAccountId))
            {
                return Unauthorized();
            }

            try
            {
                requestData.CreatedByUserAccountId = userAccountId;
                var result = _supportAccountUserLogic.CreateUser(requestData);
                return this.Json(result);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(string.Empty);
        }

        [JwtTokenAuthentication]
        [HttpGet]
        public IHttpActionResult GetMembershipByRole(long supportAccountId, long userAccountId, long roleId, long number, long size, string sort)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId))
            {
                return Unauthorized();
            }
            try
            {
                var result = _supportAccountUserLogic.GetMembershipByRole(supportAccountId, roleId, number, size, sort);
                return this.Json(result);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(string.Empty);
        }

        [JwtTokenAuthentication]
        [HttpPost]
        public IHttpActionResult NotifySuperUser([FromBody] NotifySuperUserRequest requestData)
        {
            if (!JwtTokenManager.IsAuthorized(requestData.UserAccountId, requestData.SupportAccountId, ClaimType.UserAccountId))
            {
                return Unauthorized();
            }

            try
            {
                var result = _supportAccountUserLogic.NotifySuperUser(requestData);
                return this.Json(result);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(string.Empty);
        }

        #endregion rewrite - end
    }



    // Comments By LK On 02/06
    // TODO: Tech debt need to move UserDetails to Csp.Api.Model and follow the Pascal Case naming convention
    public class userdetails
    {
        public long id { get; set; }
        public string email { get; set; }
        public string fullName { get; set; }
        public bool isLoggedin { get; set; }
        public bool canImpersonate { get; set; }
        public string username { get; set; }
    }

    public class UserDetailModel
    {
        public string Email { get; set; }
        public string Username { get; set; }
        public string Key { get; set; }
    }

    // Comments By LK On 02/06
    // TODO: Tech debt need to move ResultDetails to Csp.Api.Model and follow the Pascal Case naming convention, also create a generic class
    public class Resultdetails
    {
        public string message { get; set; }
        public bool isSuccess { get; set; }
        public bool GoogleAuthEnrollmentRedirect { get; set; }
    }

}
