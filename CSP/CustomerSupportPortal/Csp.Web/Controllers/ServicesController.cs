using Csp.Api.HelperClasses;
using Csp.Api.Logic;
using Csp.Api.Model;
using Csp.Api.Model.Enums;
using Csp.Api.Services.Contracts;
using Csp.Web.Filters;
using Csp.Web.HelperClasses;
using Microsoft.Security.Application;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Xml;

namespace Csp.Web.Controllers
{
    public class ServicesController : ApiController
    {
        private readonly IApplicationSettings _applicationSettings; 
        private readonly IServiceDataAccess _serviceDataAccess;
        private readonly IUserProfileDataAccess _userProfileDataAccess;
        private readonly ISalesforceRestApiLogic _salesforceRestApiLogic;
        private readonly ISupportAccountLogic _supportAccountLogic;
        private readonly ISecurityLogic _securityLogic;
        private readonly ISoftwareUpdateLogic _softwareUpdateLogic;
        private readonly IDeviceTransferLogic _deviceDataAccess;
        private readonly IEncryptionProvider _encryptionProvider;
        private readonly IUserAccountLogic _userAccountLogic;
        private readonly IMembershipLogic _membershipLogic;
        private readonly ISalesforceService _salesforceService;
        private readonly IServiceLogic _serviceLogic;

        private readonly ServiceProxy _serviceProxy;
        private readonly string _supportServicesUri;


        private static readonly System.Net.Http.HttpClient httpClient = new System.Net.Http.HttpClient()
        {
            Timeout = TimeSpan.FromMinutes(4)
        };

        public ServicesController(IApplicationSettings applicationSettings, IServiceDataAccess serviceDataAccess, 
            IUserProfileDataAccess userProfileDataAccess, ISalesforceRestApiLogic salesforceRestApiLogic, 
            ISupportAccountLogic supportAccountLogic, ISecurityLogic securityLogic, IDeviceTransferLogic deviceDataAccess, IUserAccountLogic userAccountLogic,
            IMembershipLogic membershipLogic, ISalesforceService salesforceService, IEncryptionProvider encryptionProvider,ISoftwareUpdateLogic softwareUpdateLogic, IServiceLogic serviceLogic)

        {
            _applicationSettings = applicationSettings;
            _serviceDataAccess = serviceDataAccess;
            _userProfileDataAccess = userProfileDataAccess;
            _salesforceRestApiLogic = salesforceRestApiLogic;
            _supportAccountLogic = supportAccountLogic;
            _securityLogic = securityLogic;
            _softwareUpdateLogic = softwareUpdateLogic;
            _deviceDataAccess = deviceDataAccess;
            _serviceProxy = new ServiceProxy();
            _supportServicesUri = SiteHelper.GetSupportServicesCloudFunctionsBaseUri();
            _userAccountLogic = userAccountLogic;
            _membershipLogic = membershipLogic;
            _salesforceService = salesforceService;
            _encryptionProvider = encryptionProvider;
            _serviceLogic = serviceLogic;

        }

        /// <summary>
        /// API to read Top 3 RSS Feeds
        /// </summary>
        /// <returns></returns>
        //[HttpGet]
        //public IHttpActionResult GetRssFeeds()
        //{
        //    List<SyndicationItem> topFeeds = new List<SyndicationItem>();
        //    List<AlertInfo> myfeeds = new List<FeedInfo>();

        //    try
        //    {
        //        string url = _applicationSettings.RssFeedURL;
        //        XmlReader reader = XmlReader.Create(url);
        //        SyndicationFeed feed = SyndicationFeed.Load(reader);
        //        reader.Close();

        //        int iterate = 0;

        //        foreach (SyndicationItem item in feed.Items)
        //        {
        //            myfeeds.Add(new FeedInfo
        //            {
        //                Title = item.Title.Text,
        //                URL = item.Id,
        //                Summary = (item.Summary.Text.Replace("<p>", "")).Replace("</p>", "")
        //            });
        //            topFeeds.Add(item);
        //            iterate++;
        //            if (iterate == 3)
        //                break;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
        //    }

        //    return this.Json(myfeeds);
        //}

        /// <summary>
        /// API to read active public alert
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [JwtTokenAuthentication]
        public IHttpActionResult GetAlerts(long userAccountId) {

            if (!JwtTokenManager.IsAuthorized(userAccountId.ToString(), ClaimType.UserAccountId))
            {
                return Unauthorized();
            }
            try {                
                var returnData = _serviceLogic.GetAlertsData();
                return this.Json(returnData);
			}
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(string.Empty);
        }

        [HttpGet]
        public IHttpActionResult GetConfigLevels()
        {
            var returnData = new object[2];

            var configLevels = new List<ConfigModel>();
            int levelid = 0;
            var subLevels = new List<ConfigModel>();

            try
            {
                configLevels = _serviceDataAccess.GetConfigLevels();
                subLevels = _serviceDataAccess.GetConfigSubLevels(levelid);

                returnData[0] = configLevels;
                returnData[1] = subLevels;
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return this.Json(returnData);
        }

        [HttpGet]
        public IHttpActionResult GetConfigSubLevels()
        {
	        int levelid = 0;
            var returnData = new List<ConfigModel>();

            try
            {
                returnData = _serviceDataAccess.GetConfigSubLevels(levelid);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return this.Json(returnData);
        }

        [HttpGet]
        [JwtTokenAuthentication]
        public async Task<IHttpActionResult> GetCaseRecentActivity(string email, long accountId)
        {
            email = Sanitizer.GetSafeHtmlFragment(email);
            if (!JwtTokenManager.IsAuthorized(email))
            {
                return Unauthorized();
            }

            //Fix for CSP-707 - Removed double decoding of email as it is already decoded by ASP.NET
            //email = WebUtility.UrlDecode(email);
            var caseRecentActivityList = new List<SupportCaseInfo>();
            var caseRecentActivities = new List<CaseActivity>();
            var salesforceCaseDetailUrl = ConfigurationManager.AppSettings["SalesforceCaseDetailUrl"];
            
            try
            {
                DateTime userLastLoginDateTime = DateTime.MinValue;
                var userProfile = _userProfileDataAccess.GetUserData(email);
                var userAccountId = userProfile.UserAccountId.HasValue ? userProfile.UserAccountId.Value : 0;
                var supportAccount = _supportAccountLogic.GetSupportAccount(accountId, userAccountId);
                var hasCaseManagementAccess = _securityLogic.HasViewCaseManagementPrivilege(accountId, userAccountId);

                if (accountId == ApiConstants.PANW_SUPPORT_ACCOUNT_ID)
                {
                    //Cache user's last login date
                    object cachedData = HttpRuntime.Cache.Get(email + "userLastLoginDate");
                    if (cachedData == null)
                    {
                        
                        userLastLoginDateTime = await _salesforceRestApiLogic.GetUserLastLoginDateTime(userAccountId, accountId);
                       
                        HttpRuntime.Cache.Insert(email + "userLastLoginDate", userLastLoginDateTime);
                        HttpRuntime.Cache.Insert(email+ "userLastLoginDate", userLastLoginDateTime, null, System.Web.Caching.Cache.NoAbsoluteExpiration, TimeSpan.FromHours(1));
                    }
                    else
                    {
                       userLastLoginDateTime = (DateTime)cachedData;
                    }
                    
                }
                else
                {
                    userLastLoginDateTime = _userProfileDataAccess.GetUserLastLoginDateTime(email);
                }

                if (!hasCaseManagementAccess || string.IsNullOrEmpty(supportAccount.SalesforceId))
                {
                    return this.Json(caseRecentActivityList); 
                }

                if (userLastLoginDateTime != DateTime.MinValue)
                {
                    object cachedCaseData = HttpRuntime.Cache.Get(email + accountId + "caseRecentActivityList");
                    if (cachedCaseData == null)
                    {
                        var contactId = "";

                        if (SiteHelper.IsSalesforceDefaultAccount(supportAccount.SalesforceId))
                        {
                            contactId =
                                _membershipLogic.GetSalesforceContactIdByUserAndSupportAccountId(userAccountId,
                                    accountId);
                        }

                        caseRecentActivityList = await _salesforceRestApiLogic.GetCaseRecentActivity(supportAccount.SalesforceId, contactId, userLastLoginDateTime);

                        HttpRuntime.Cache.Insert(email + accountId + "caseRecentActivityList", caseRecentActivityList);
                        HttpRuntime.Cache.Insert(email + accountId + "caseRecentActivityList", caseRecentActivityList, null, System.Web.Caching.Cache.NoAbsoluteExpiration, TimeSpan.FromHours(1));
                    }
                    else
                    {
                        caseRecentActivityList = (List<SupportCaseInfo>)cachedCaseData;
                    }
                }

                foreach (var item in caseRecentActivityList)
                {
                    var feedInfo = new CaseActivity();

                    feedInfo.Title = string.Format("{0} AT {1}", item.LastModifiedDate.ToShortDateString(), item.LastModifiedDate.ToShortTimeString());
                    feedInfo.URL = string.Format(salesforceCaseDetailUrl, item.Id);

                    var subject = GetCaseSubject(item.Subject);

                    if (item.Status == "StatusChange")
                    {
                        feedInfo.Summary = string.Format("Status of case {0} - {1} changed from {2} to {3}", item.CaseNumber, subject, item.CaseHistory.OldStatus, item.CaseHistory.NewStatus);
                    }
                    else if (item.Status == "New")
                    {
                        feedInfo.Summary = string.Format("{0} has created a new support case: {1} - {2}", item.CreatedBy, item.CaseNumber, subject);
                    }
                    else if (item.Status == "Reopen")
                    {
                        feedInfo.Summary = string.Format("{0} has reopned support case: {1} - {2}", item.LastModifiedBy, item.CaseNumber, subject);
                    }
                    else if (item.Status == "Customer Comment Added")
                    {
                        feedInfo.Summary = string.Format("{0} has added a new comment for support case: {1} - {2}", item.LastModifiedBy, item.CaseNumber, subject);
                    }
                    else if (item.Status == "Awaiting Customer Verification")
                    {
                        feedInfo.Summary = string.Format("Support case {0} is awaiting customer verification", item.CaseNumber);
                    }
                    else if (item.Status == "Customer Action Requested")
                    {
                        feedInfo.Summary = string.Format("Customer action requested for support case {0}", item.CaseNumber);
                    }
                    else if (item.Status == "Pending Customer")
                    {
                        feedInfo.Summary = string.Format("Support case {0} is pending customer's input", item.CaseNumber);
                    }
                    else if (item.Status == "Closed")
                    {
                        feedInfo.Summary = string.Format("{0} has closed support case: {1} - {2}", item.CreatedBy, item.CaseNumber, subject);
                    }

                    caseRecentActivities.Add(feedInfo);
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return this.Json(caseRecentActivities);
        }

        
       

        private string GetCaseSubject(string subject)
        {
            if (subject.Length > 50)
            {
                return string.Format("{0}...", subject.Substring(0, 50));
            }

            return subject;
        }

        /**
         * This method will proxy the current HttpRequestMessage to the indicated URL with the specified authorization
         */
        private async Task<HttpResponseMessage> sendProxyRequest(string reqUrl, string authorization = "", NameValueCollection httpHeaders = null)
        {
            var userEmail = HttpContext.Current.User.Identity.Name;

            HttpResponseMessage response = null;

            if (reqUrl.Contains("upload"))
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(string.Format("Before sending request to bpa api, time:{0}, email:{1},requrl:{2} ", DateTime.Now, userEmail, reqUrl)));
            }

            try
            {
                response = await _serviceProxy.Request(
                    Request.Method,
                    reqUrl,
                    userEmail,
                    Request.Content,
                    authorization,
                    httpHeaders);

                return response;
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(string.Format("Exception response, time:{0}, email:{1} ", DateTime.Now, userEmail)));
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                var resError = new HttpResponseMessage(response == null ? HttpStatusCode.ServiceUnavailable : response.StatusCode);
                resError.Content = new StringContent(ex.Message);

                return resError;
            }
        }

        private string getServiceAccountJwt(string audience)
        {
            var cacheKeyName = "ServiceAccountJwt";
            var jwt = HttpContext.Current.Cache.Get(cacheKeyName);

            if (jwt == null)
            {
                double expiresInSecs;
                jwt = (object)_serviceProxy.CreateServiceAccountJwt(audience, out expiresInSecs);

                HttpContext.Current.Cache.Add(cacheKeyName, jwt, null,
                    DateTime.Now.AddSeconds(expiresInSecs - 60),
                    System.Web.Caching.Cache.NoSlidingExpiration,
                    System.Web.Caching.CacheItemPriority.Normal, null);
            }

            return jwt.ToString();
        }

        [AcceptVerbs("GET", "POST")]
        [JwtTokenAuthentication, Csp.Web.Attributes.AjaxOnly, System.Web.Mvc.ValidateAntiForgeryToken]
        public async Task<IHttpActionResult> BestPracticeAssessment(long supportaccountid = 0, string serial = "")
        {
            var method = Request.Method;
            // Removing: /proxy/bestpracticeassessment : Index 28
            var reqUrl = string.Format("{0}{1}", SiteHelper.GetBPABaseUri(), Request.RequestUri.PathAndQuery.Substring(29));
            var userEmail = HttpContext.Current.User.Identity.Name;
            var authorized = true;

            if (reqUrl.Contains("upload"))
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(string.Format("BestPracticeAssessment: time:{0}, email:{1},requrl:{2} ", DateTime.Now, userEmail, reqUrl)));
            }

            if (supportaccountid != 0 || reqUrl.Contains("/report/archive/"))
            {
                // The request needs to be authorized

                var userProfile = _userProfileDataAccess.GetUserData(userEmail);
                var userAccountId = userProfile.UserAccountId.HasValue ? userProfile.UserAccountId.Value : 0;
                var userSupportAccount = _supportAccountLogic.GetSupportAccount(supportaccountid, userAccountId);

                if (supportaccountid != 0 && userSupportAccount.Id != supportaccountid)
                {
                    authorized = false;
                }

                if (reqUrl.Contains("/report/archive/") && !_securityLogic.IsSuperUser(supportaccountid, userAccountId))
                {
                    authorized = false;
                }
            }

            if (!authorized) return this.Content(HttpStatusCode.Unauthorized, "You do not have access to the requested operation.");

            var proxyResponse = await sendProxyRequest(reqUrl, string.Format("Token {0}", SiteHelper.GetBPAAuthKey()));

            if (proxyResponse.StatusCode != HttpStatusCode.ServiceUnavailable && proxyResponse.Content != null)
            {
                if (reqUrl.Contains("/report/generate/") && method == System.Net.Http.HttpMethod.Get)
                {
                    // Captures the generation of the BPA for history purpose
                    var content = await proxyResponse.Content.ReadAsStringAsync();
                    dynamic bpaResult = Newtonsoft.Json.JsonConvert.DeserializeObject(content);
                    var serialNumber = serial;
                    if (bpaResult.serial != null) serialNumber = (string)bpaResult.serial;
                    _serviceDataAccess.CaptureBPA((string)bpaResult.task_id, userEmail, serialNumber);
                }
            }

            return ResponseMessage(proxyResponse);
        }

        // Endpoint for Day-One config generation
        [AcceptVerbs("POST")]
        [JwtTokenAuthentication, Csp.Web.Attributes.AjaxOnly, System.Web.Mvc.ValidateAntiForgeryToken]
        public async Task<IHttpActionResult> LoadableConfigurations(long supportaccountid = 0)
        {
            var reqUrl = string.Format("{0}{1}", _supportServicesUri, Request.RequestUri.PathAndQuery.Replace("/proxy/", "/"));

            var proxyResponse = await sendProxyRequest(reqUrl, string.Format("Bearer {0}", getServiceAccountJwt(reqUrl)));
            if (proxyResponse.Content != null && proxyResponse.StatusCode == HttpStatusCode.OK)
            {
                var userEmail = HttpContext.Current.User.Identity.Name;
                var toList = new List<string>();
                toList.Add(userEmail);
                
                var userProfile = _userProfileDataAccess.GetUserData(userEmail);

                EmailHelper.QueueEmail((int)TemplateId.DayOneConfig, toList, supportaccountid, "LoadableConfigurations", ApiConstants.DAY_ONE_CONFIG_NOTIFICATION, null, null, userProfile.FirstName, userProfile.LastName);
            }

            return ResponseMessage(proxyResponse);
        }

        // Endpoint for logging support events
        [AcceptVerbs("POST")]
        [JwtTokenAuthentication, Csp.Web.Attributes.AjaxOnly, System.Web.Mvc.ValidateAntiForgeryToken]
        public async Task<IHttpActionResult> SupportEventsLogger()
        {
            var reqUrl = string.Format("{0}{1}", _supportServicesUri, Request.RequestUri.PathAndQuery.Replace("/proxy/", "/"));
            
            var response = await this.sendProxyRequest(reqUrl, string.Format("Bearer {0}", getServiceAccountJwt(reqUrl)));

            return this.ResponseMessage(response);
        }
        
        [HttpPost, HttpOptions]
        public SeLabKitAccountDetails GetSeLabKitSupportAccountId([FromBody]SeLabKitAccountModel model)
        {
            var accountDetails = new SeLabKitAccountDetails();

            try
            {
                var privateKey = _applicationSettings.EncryptionProviderPrivateKey;
                var isKeyValid = _encryptionProvider.IsKeyValid(model.Email, model.Key, privateKey);

                if (!isKeyValid)
                {
                    return new SeLabKitAccountDetails()
                    {
                        AccountId = 0,
                        Description = "Unauthorized. Invalid Token"
                    };
                }

                // Check if user is a Pan Employee and if he exists in UserAccounts.
                if (string.IsNullOrEmpty(model.Email) ||
                    !model.Email.ToLower().Contains(ApiConstants.PANW_EMPLOYEE_EMAIL_EXTENSION) ||
                    !_securityLogic.IsPanEmployeeRegisteredToSupportSite(model.Email))
                {
                    return new SeLabKitAccountDetails()
                    {
                        AccountId = 0,
                        Description = "Invalid User"
                    };
                }

                var userAccount = _userAccountLogic.GetUserAccountByEmail(model.Email);
                var salesforceAccountId = _applicationSettings.SalesforceDefaultSeAccountId;
                var accountTypeId = (int)Api.Model.Enums.AccountType.PanEmployee;

                if (userAccount == null)
                {
                    return new SeLabKitAccountDetails()
                    {
                        AccountId = 0,
                        Description = "User not found"
                    };
                }

                var supportAccount = _supportAccountLogic.GetSeLabKitSupportAccountId(userAccount.Id);

                if (supportAccount == null)
                {
                    string accountName = string.Format("PAN SE - [{0}]", model.Email);

                    supportAccount = _supportAccountLogic.CreateSeLabKitSupportAccount(userAccount.Id, salesforceAccountId, accountTypeId, accountName, 
                        userAccount.PhoneNumber, userAccount.Address.Address1, userAccount.Address.Address2, userAccount.Address.City,
                        userAccount.Address.Region, userAccount.Address.PostalCode, userAccount.Address.CountryId, ApiConstants.PANW_SYSTEM_USER_ACCOUNT_ID);

                    if (supportAccount != null) 
                    {
                        var membership = new Membership()
                        {
                            UserAccount = new UserAccount() { Id = userAccount.Id },
                            SupportAccountId = supportAccount.Id,
                            ActivationDate = DateTime.Now,
                            Description = "New user created",
                            CreatedOn = DateTime.Now,
                            CreatedByEmail = ApiConstants.PANW_SYSTEM_USER_EMAIL,
                            CreatedByUserAccountId = ApiConstants.PANW_SYSTEM_USER_ACCOUNT_ID
                        };

                        // Create Membership
                        membership = _membershipLogic.CreateMembership(membership);
                        var membershipId = membership.Id;

                        // Assign Membership Roles
                        _membershipLogic.AssignMembershipRoles(membershipId, userAccount.Id, supportAccount.Id,
                                                                                                ApiConstants.PANW_SYSTEM_USER_ACCOUNT_ID);
                        // Salesforce Contact and Portal Username Creation will be handled by the scheduled job that runs every 5 minutes

                        accountDetails.AccountId = supportAccount.Id;
                        accountDetails.Name = supportAccount.Name;
                        accountDetails.Description = "New SE support account created";
                    }
                }
                else
                {
                    accountDetails.AccountId = supportAccount.Id;
                    accountDetails.Name = supportAccount.Name;
                    accountDetails.Description = "SE Support account already exists";
                }
            }
            catch (Exception ex)
            {
                var errorMsg = "Error in call to GetSELabKitSupportAccountId";

                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(errorMsg));
            }
            return accountDetails;
        }

        // Endpoint for logging support events
        [AcceptVerbs("POST", "PATCH")]
        [JwtTokenAuthentication, Csp.Web.Attributes.AjaxOnly, System.Web.Mvc.ValidateAntiForgeryToken]
        public async Task<IHttpActionResult> NotificationsAccessor()
        {
            var baseUrl = ConfigurationManager.AppSettings["NotificationsAccessorBaseUri"].ToString();

            var reqUrl = string.Format("{0}{1}", baseUrl, Request.RequestUri.PathAndQuery.Replace("/proxy/notificationsAccessor", ""));
            var userProfile = _userProfileDataAccess.GetUserData(HttpContext.Current.User.Identity.Name);          
            if (HttpContext.Current.Request.Headers["PAN-Support-Account-Id"] != null)
            {
                var headers = new NameValueCollection(2);
                headers.Add("PAN-Support-Account-Id", HttpContext.Current.Request.Headers["PAN-Support-Account-Id"]);
                headers.Add("PAN-User-Account-Id", userProfile.UserAccountId.ToString());

                var oidcToken = IAPClient.CreateOIDCToken(baseUrl);
                var response = await this.sendProxyRequest(reqUrl, string.Format("Bearer {0}", oidcToken), headers);

                // HTTP Headers for testing
                // response.Headers.Add("pan-oidc-token", oidcToken);
                // response.Headers.Add("pan-notifications-uri", reqUrl);
                // response.Headers.Add("pan-user-account-id", userProfile.UserAccountId.ToString());
                // End

                return this.ResponseMessage(response);
            }
            else
            {
                return this.Content(HttpStatusCode.BadRequest, "Support account is missing");
            }
        }
    }
}
