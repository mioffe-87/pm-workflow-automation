using Newtonsoft.Json;
using System;
using System.Collections;
using System.Configuration;
using System.Dynamic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Csp.Api.HelperClasses;
using Csp.Api.Services.Contracts;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Mail;
using System.IO;
using System.Linq;
using Csp.Api.Logic;
using System.Web.Script.Serialization;
using Csp.Api.Model;
using System.ComponentModel;

namespace Csp.Web.HelperClasses {
	public static class SiteHelper {
		public const string DateFormat = "MM/dd/yyyy";
		public const string DateFormatString = "{0:MM/dd/yyyy}";
		public const int DefaultPageSize = 10;

		public static readonly string[] ValidAscScorecardUploadDateFormats =
	    {
	        "yyyy-MM-dd", "yyyy-M-d", "yyyy-MM-d",
	        "yyyy-M-dd"
	    };

		public static string GetBaseUrl() {
			var host = HttpContext.Current.Request.Url.Host;
			var port = HttpContext.Current.Request.Url.IsDefaultPort ? string.Empty : string.Format(":{0}", HttpContext.Current.Request.Url.Port);

			if (HttpContext.Current.Request.IsLocal) {
				if (!host.Contains("paloaltonetworks.com")) {
					host = HttpContext.Current.Request.Url.Host + ".paloaltonetworks.com";
				}
			}

			return string.Format("{0}://{1}{2}", HttpContext.Current.Request.Url.Scheme, host, port);
		}

		public static string GetCoveoSearchToken(string email = null) {
			var coveoToken = string.Empty;

			try {
				// Comments By LK On 02/22/2018
				// GetBaseUrl HttpContext.Current.Request.Url.Scheme is returning http
				// var baseAddress = GetBaseUrl();
				var baseAddress = ConfigurationManager.AppSettings["ApplicationUrl"].ToString(); //GetBaseUrl();
				var requestUri = new Uri(string.Format("{0}/api/CoveoSearchToken", baseAddress));
				dynamic searchConfig = new ExpandoObject();

				// Get the user's email
				if (!string.IsNullOrEmpty(email)) {
					searchConfig.EmailAddress = email;
				} else { // check if user context already exists from a logged in session
					searchConfig.EmailAddress = (HttpContext.Current.User != null) ? HttpContext.Current.User.Identity.Name : string.Empty;
				}

				searchConfig.SupportAccountId = CurrentState.GetSupportAccountId() ?? 0;

                string json = "";
				json = JsonConvert.SerializeObject(searchConfig);
				var httpClient = new System.Net.Http.HttpClient();
				var httpResponse = httpClient.PostAsync(requestUri, new StringContent(json, Encoding.UTF8, "application/json"));

                if (httpResponse.Result.StatusCode == HttpStatusCode.OK) {
                    var coveoTokenResult = httpResponse.Result.Content.ReadAsStringAsync();
                    coveoToken = coveoTokenResult.Result;
                }
            } catch (Exception ex) {
				Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
			}

			return coveoToken;
		}

		public static void UpdateNotificationMessageStatus(string notificationId = "", string entityId = "", string category = "") {
			try {
				long userAccountId = GetUserAccountId();
				long supportAccountId = CurrentState.GetSupportAccountId() ?? 0;
				var baseUrl = ConfigurationManager.AppSettings["NotificationsAccessorBaseUri"];
				var requestUri = !string.IsNullOrEmpty(notificationId) ? string.Format("{0}/notifications/{1}", baseUrl, notificationId) : string.Format("{0}/notifications?entityId={1}&category={2}", baseUrl, entityId, category);
				var oidcToken = IAPClient.CreateOIDCToken(baseUrl);

				var request = new HttpRequestMessage {
					Method = new HttpMethod("PATCH"),
					RequestUri = new Uri(requestUri)
				};

				request.Headers.Add("PAN-Support-Account-Id", supportAccountId.ToString());
				request.Headers.Add("PAN-User-Account-Id", userAccountId.ToString());
				request.Headers.Add("Authorization", string.Format("Bearer {0}", oidcToken));

				request.Content = new StringContent(
					JsonConvert.SerializeObject(new { status = "Done" }),
					Encoding.UTF8, "application/json");

				var httpClient = new System.Net.Http.HttpClient();
				var response = httpClient.SendAsync(request);

				var status = response.Result;
			} catch (Exception ex) {
				Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
			}
		}

        public static string GetAuthToken(string email = null)
        {
            var authToken = string.Empty;

            if (!string.IsNullOrEmpty(email))
            {
                authToken = GenerateToken(email);
                LoginUserHistory(email);
            }
            else
            {
                var jsonSerializer = new JavaScriptSerializer();
                authToken = jsonSerializer.Serialize(new JwtTokenResponse()
                {
                    AccessToken = string.Empty,
                    RefreshToken = string.Empty
                });
            }
            return authToken;
        }

        public static string GetResourceRevisionNumber() {
            string theme = GetUserSelectedTheme();
            string configKey = theme == UserViewSetting.CLASSIC.ToDescription()
                ? "ResourceRevisionNumber"
                : "ResourceNextGenRevisionNumber";
            return ConfigurationManager.AppSettings[configKey];
        }

	// TO DO update the CDN url
	public static string GetPortalAssetsCdnUrl() {
		string theme = GetUserSelectedTheme();
		string configKey = theme == UserViewSetting.CLASSIC.ToDescription()
			? "PortalAssetsCdnUrl"
			: "PortalAssetsNextGenCdnUrl";
		return ConfigurationManager.AppSettings[configKey];
	}

		public static string GetUserTypeKey() {
			return ConfigurationManager.AppSettings["CoveoUserTypeKey"];
		}

		public static string GetUserType(string email = null) {
			string userType = ConfigurationManager.AppSettings["CoveoUserTypeExternal"];

			if (!string.IsNullOrEmpty(email) && email.ToLower().Contains(ApiConstants.PANW_EMPLOYEE_EMAIL_EXTENSION)
				&& email != ApiConstants.PANW_COVEO_ANONYMOUS_USER_EMAIL) {
				userType = ConfigurationManager.AppSettings["CoveoUserTypeInternal"];
			}

			return userType;
		}
        public static string GetCoveoSearchPageUrl()
        {
            string coveoSearchPageUrl= ConfigurationManager.AppSettings["CoveoSearchPageUrl"].ToString();
            var supportAccountId = CurrentState.GetSupportAccountId() ?? 0;
            return (supportAccountId > 0) ? coveoSearchPageUrl+"/"+ supportAccountId
                : coveoSearchPageUrl;
        }

        

        public static string GetCoveoEnvType() {
			return ConfigurationManager.AppSettings["CoveoInstance"];
		}

		public static string GetCoveoSearchDataSource() {
			return ConfigurationManager.AppSettings["CoveoSearchDataSource"].ToString();
		}

		public static string GetCoveoSelfHelpDataSource() {
			return ConfigurationManager.AppSettings["CoveoSelfHelpDataSource"].ToString();
		}

		public static string GetCoveoRelatedThreadsDataSource() {
			return ConfigurationManager.AppSettings["CoveoRelatedThreadsDataSource"].ToString();
		}

		public static string GetBPABaseUri() {
			return ConfigurationManager.AppSettings["BPABaseUri"].ToString();
		}

		public static string GetBPAAuthKey() {
			return ConfigurationManager.AppSettings["BPAAuthKey"].ToString();
		}

		public static long GetUserAccountId() {
			var userAccountId =
					(CurrentState.GetLoggedInUser() != null && CurrentState.GetLoggedInUser().UserAccount != null)
							? CurrentState.GetLoggedInUser().UserAccount.Id
							: 0;

			return userAccountId;
		}

		public static bool IsSupportOnlyAccount() {
			var salesforceRestApiLogic = DependencyResolver.Current.GetService<ISalesforceRestApiLogic>();
			var supportAccountLogic = DependencyResolver.Current.GetService<ISupportAccountLogic>();
			var isSupportOnlyAccount = false;
			var supportAccountId = CurrentState.GetSupportAccountId() ?? 0;
			var salesforceAccountId = supportAccountLogic.GetSalesforceIdBySupportAccountId(supportAccountId);

			if (!string.IsNullOrEmpty(salesforceAccountId)) {
				var task = Task.Run(async () => await salesforceRestApiLogic.IsSupportOnlyAccount(salesforceAccountId));

				isSupportOnlyAccount = task.Result;
			}

			return isSupportOnlyAccount;
		}

		public static string GetSupportServicesCloudFunctionsBaseUri() {
			return ConfigurationManager.AppSettings["SupportServicesCloudFunctions"].ToString();
		}

		public static bool IsThirdPartyIdpConfigured(string email) {
			bool isConfigured = false;
			string domain = string.Empty;

            if (string.IsNullOrEmpty(email))
                return isConfigured;

			if (email.Contains("@"))
				domain = new MailAddress(email).Host;
			else
				domain = email;

			try {
				dynamic configuration = GetThirdPartyIdpConfiguration(domain);

                if (configuration != null)
                {
                    if (configuration.domain != null)
                        isConfigured = domain.Equals(Convert.ToString(configuration.domain), StringComparison.InvariantCultureIgnoreCase);
                }
               
			} catch (Exception ex) {
				Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
			}

			return isConfigured;
		}

		public static dynamic GetThirdPartyIdpConfiguration(string domain) {
			dynamic configuration = null;

            //Enable third party idp for Beta customers only
            var supportAccountLogic = DependencyResolver.Current.GetService<ISupportAccountLogic>();            
			var thirdPartyIdpConfigUrl = ConfigurationManager.AppSettings["ThirdPartyIdpConfigUrl"];
			var thirdPartyIdpCertThumbprint = ConfigurationManager.AppSettings["ThirdPartyIdpCertThumbprint"];

			try {
				ServicePointManager.Expect100Continue = true;
				ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
				ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
																							 | SecurityProtocolType.Tls11
																							 | SecurityProtocolType.Tls12
																							 | SecurityProtocolType.Ssl3;

				var request = (HttpWebRequest)WebRequest.Create(string.Format("{0}/{1}", thirdPartyIdpConfigUrl, domain));
				request.ContentType = "application/json; charset=utf-8";
				request.Method = "GET";

				X509Certificate2 clientCertificate = CertificateManager.GetCertificateByCnName(thirdPartyIdpCertThumbprint);

				if (clientCertificate == null) {
					Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception("Empty certificate store"));
				}

				request.ClientCertificates.Add(clientCertificate);

				using (var response = request.GetResponse()) {
					var statusCode = ((HttpWebResponse)response).StatusCode;
					if (statusCode == HttpStatusCode.OK) {
						using (var reader = new StreamReader(response.GetResponseStream())) {
							string jsonResponse = reader.ReadToEnd();

							if (!string.IsNullOrEmpty(jsonResponse)) {
								var result = JsonConvert.DeserializeObject<dynamic>(jsonResponse);
								if (result.data != null) {
									configuration = result.data;
								}
							}
						}
					}
				}
			} catch (Exception ex) {
				Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
			}

			return configuration;
		}

        public static bool IsSalesforceDefaultAccount(string accountId) {
            var defaultAccountsList = new ArrayList { ConfigurationManager.AppSettings["SalesforceDefaultAccountId"], ConfigurationManager.AppSettings["SalesforceDefaultAWSAccountId"], ConfigurationManager.AppSettings["SalesforceDefaultAzureAccountId"], ConfigurationManager.AppSettings["SalesforceDefaultGoogleCloudAccountId"] };

            var defaultAccountIds = from string defaultAccountId in defaultAccountsList
                select defaultAccountId.Substring(0, 15);

            accountId = (!string.IsNullOrEmpty(accountId) && accountId.Length > 15)
                ? accountId.Substring(0, 15)
                : accountId;

            return defaultAccountIds.Contains(accountId);
        }

        private static string GenerateToken(string email)
        {
            var jwtTokenResponse = JwtTokenManager.GenerateToken(email);
            var jsonSerializer = new JavaScriptSerializer();
            return jsonSerializer.Serialize(jwtTokenResponse);
        }

        public static async Task<bool> LoginUserHistory(string email)
        {
            #region check & log

            try
            {
                HttpCookie shistCookie = HttpContext.Current.Request.Cookies[ApiConstants.Login_User_Session_Identifier];
                if (shistCookie != null)
                {
                    var loginHistoryLogic = DependencyResolver.Current.GetService<ILoginHistoryLogic>();
                    var loginDetails = loginHistoryLogic.GetLoginUserHistory(shistCookie.Value);

                    if (!string.IsNullOrEmpty(loginDetails?.SessionId)) return false;


                    var loginHistory = new LoginHistoryModel()
                    {
                        SessionId = shistCookie.Value,
                        UserAccountId = GetUserAccountId(),
                        SupportAccountId = CurrentState.GetSupportAccountId() ?? 0,
                        Browser = HttpContext.Current.Request.Browser.Browser,
                        Platform = HttpContext.Current.Request.Browser.Platform,
                        SourceIpAddress = HttpContext.Current.Request.UserHostAddress,
                        LoginTime = DateTime.Now,
                        LoginTimeAsString = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    };

                    loginHistoryLogic.AddLoginUserHistory(loginHistory);

                    await ValidateSfdcContact(email);
                }
            }
            catch (Exception ex)
            {
                if (HttpContext.Current != null)
                {
                    Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                }
                else
                {
                    Elmah.ErrorLog.GetDefault(null).Log(new Elmah.Error(ex));
                }
            }
            return true;
            #endregion
        }

        private static async Task<bool> ValidateSfdcContact(string email)
        {
            var response = true;
            long supportAccountId = CurrentState.GetSupportAccountId() ?? 0;
            long userAccountId = GetUserAccountId();

            if (supportAccountId > 0 && userAccountId > 0)
            {
                var supportAccountUserLogic = DependencyResolver.Current.GetService<ISupportAccountUserLogic>();
                await supportAccountUserLogic.ValidateSfdcContact(supportAccountId, userAccountId, email);
            }
            return response;
        }

		public static string GetUserSelectedTheme()
		{
            long userAccountId = GetUserAccountId();
			if (userAccountId > 0)
			{
				string theme = CurrentState.GetUserSelectedTheme();
				return string.IsNullOrEmpty(theme) ? UserViewSetting.LIGHT.ToDescription() : theme;
            }
			return UserViewSetting.LIGHT.ToDescription();
        }

        public enum UserViewSetting
        {
            [Description("CLASSIC")]
            CLASSIC = 1,
            [Description("DARK")]
            DARK = 2,
            [Description("LIGHT")]
            LIGHT = 3,
        }
    }
}