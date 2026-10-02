using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Web;
using Csp.Api.EntityClasses;
using Csp.Api.HelperClasses;
using Csp.Api.LoginService;
using Csp.Api.Model;
using Csp.Api.Model.Enums;
using Csp.Api.Services.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using Csp.Api.DataAccess;

namespace Csp.Api.Logic {
	public class LoginServiceLogic : ILoginServiceLogic {
		private readonly ILogLogic _logLogic;
		private readonly IApplicationSettings _applicationSettings;
        private readonly string _loginServiceURL = ConfigurationManager.AppSettings.Get("LoginServiceUrl");
        private readonly string _loginServiceThumbprint = ConfigurationManager.AppSettings.Get("LoginServiceThumbprint");
        private readonly string _loginServiceUser = ConfigurationManager.AppSettings.Get("LoginServiceUserName");
        private readonly string _loginServiceKey = ConfigurationManager.AppSettings.Get("LoginServicePassword");

		public LoginServiceLogic() {

		}

		public LoginServiceLogic(ILogLogic logLogic, IApplicationSettings applicationSettings) {
			_logLogic = logLogic;
			_applicationSettings = applicationSettings;
		}

        private dynamic GetUserProfileResponse(HttpWebRequest request, bool isCheckForProfileExist)
        {
            var userDetails = new UserProfile();
            using (var profileResponse = request.GetResponse())
            {
                HttpStatusCode statusCode = ((HttpWebResponse) profileResponse).StatusCode;
                if (statusCode == HttpStatusCode.OK)
                {
                    using (var reader = new StreamReader(profileResponse.GetResponseStream()))
                    {
                        if (isCheckForProfileExist)
                        {
                            dynamic data = JObject.Parse(reader.ReadToEnd());
                            return data != null ? Convert.ToBoolean(data.profileExists): false;
                        }
                        var responsePayload = reader.ReadToEnd();
                        userDetails = JsonConvert.DeserializeObject<UserProfile>(responsePayload,
                            new JsonSerializerSettings
                            {
                                NullValueHandling = NullValueHandling.Ignore,
                                MissingMemberHandling = MissingMemberHandling.Ignore
                            });
                    }
                }
            }
            return userDetails;
        }

        public bool IsProfileExists(string emailAddress, string ipAddress) {
            var url = String.Format("{0}/v1/profiles?email={1}&ipAddress={2}", _loginServiceURL,emailAddress, ipAddress);  
            var webRequest = WebCommon.GetWebRequestWithClientCertificate(url, "GET", _loginServiceThumbprint, string.Empty, _loginServiceUser, _loginServiceKey);
            return GetUserProfileResponse(webRequest, true);
        }

		public UserProfile GetUserProfile(string emailAddress, string ipAddress, string key)
        {

            var md5HashKey = BaseLogic.CalculateMd5Hash(string.Format("{0}|{1}", emailAddress, key));
            var url = String.Format("{0}/v1/users?email={1}&ipAddress={2}&key={3}", _loginServiceURL, emailAddress, ipAddress, md5HashKey);
            var webRequest = WebCommon.GetWebRequestWithClientCertificate(url, "GET", _loginServiceThumbprint, string.Empty, _loginServiceUser, _loginServiceKey);
            return GetUserProfileResponse(webRequest, false);
		}

        public UserProfile GetUserProfileV2(string emailAddress)
        {
            UserProfile userProfile =null;
            string baseUrl = string.Format(ApiConstants.LoginUserServiceBaseUrl, _loginServiceURL, emailAddress);
            var method = HttpMethod.Get.Method;
            try
            {
               userProfile = CommonHelper.GetWebResponse<UserProfile>(baseUrl, method, _loginServiceThumbprint, string.Empty, true, _loginServiceUser, _loginServiceKey);
            }
            catch (Exception ex)
            {
                LogApiErrors(baseUrl, method, string.Empty, "Exception", ex.ToString());
            }
            return userProfile;
        }

        public UserProfile Register(string emailAddress, string password, string firstName, string lastName,
																string companyName, string addressLine1, string addressLine2, string city,
																string region, string postalCode, string countryCode, string phoneNumber,
																string ipAddress, bool isStandalone = false) {
            var url = _loginServiceURL + "/v2/users";
            string method = "POST";
            var userPayload = new
            {
                email = emailAddress,
                password,
                firstName,
                lastName,
                company = companyName ?? string.Empty,
                address1 = addressLine1,
                address2 = addressLine2,
                city,
                state = region,
                zipCode = postalCode,
                country = countryCode,
                phone = phoneNumber,
                //uniqueIdentifer = Guid.NewGuid().ToString(),
                appId = ApplicationType.Support.ToDescription().ToLower(),
                createdBy = emailAddress,
                ipAddress,
                isStandAlone = isStandalone,
                userType = ApiConstants.UserType
            };
            var jsonPayload = JsonConvert.SerializeObject(userPayload);
            var webRequest = WebCommon.GetWebRequestWithClientCertificate(url, method, _loginServiceThumbprint, jsonPayload, _loginServiceUser, _loginServiceKey);

            try
            {
                return GetUserProfileResponse(webRequest, false);
            }
            catch (Exception ex)
            {
                LogApiErrors(url, method, jsonPayload, "Exception", ex.ToString());
                throw;
            }
        }

        public bool ManageGoogleAuth(string crudType, out string responseMessage, string email = "", GoogleAuthRequest googleAuthRequest = null)
        {
            responseMessage = string.Empty;
            string method = string.Empty;
            string jsonPayload = string.Empty;
            dynamic result = null;
            bool status = false;
            string baseUrl = string.Format(ApiConstants.GoogleAuthEndpoint, _loginServiceURL, email);

            if (crudType.ToLower() == ManageUserAction.Get.ToDescription())
            {
                method = HttpMethod.Get.Method;
            }
            else if (crudType == ManageUserAction.Create.ToDescription())
            {
                method = HttpMethod.Post.Method;
                jsonPayload = JsonConvert.SerializeObject(googleAuthRequest);
            }
            else if (crudType == ManageUserAction.Update.ToDescription())
            {
                method = new HttpMethod("PATCH").Method;
                jsonPayload = JsonConvert.SerializeObject(googleAuthRequest);
            }
            else if (crudType.ToLower() == ManageUserAction.Delete.ToDescription())
            {
                method = HttpMethod.Delete.Method;
            }
            try
            {
                result = CommonHelper.GetWebResponse<GoogleAuthResponse>(baseUrl, method, _loginServiceThumbprint, jsonPayload, true, _loginServiceUser, _loginServiceKey);
                status = (!string.IsNullOrEmpty(result?.Email)) ? true : false;
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException<dynamic>(ex);
                if((crudType.ToLower() == ManageUserAction.Get.ToDescription() && result?.message?.ToString().ToLower() != ApiConstants.UserNotFound.ToLower()) ||
                    crudType.ToLower() == ManageUserAction.Delete.ToDescription())
                {
                    responseMessage = ApiConstants.GoogleAuthFailedMessage;
                }
            }
            catch
            {
                responseMessage = ApiConstants.GoogleAuthFailedMessage;
            }
            return status;
        }

        public UserProfile ActivateUser(string emailAddress)
        {

            var url = String.Format("{0}/v2/users/activate/{1}", _loginServiceURL, emailAddress);
            var webRequest = WebCommon.GetWebRequestWithClientCertificate(url, "POST", _loginServiceThumbprint, string.Empty, _loginServiceUser, _loginServiceKey);
            return GetUserProfileResponse(webRequest, false);
        }

        public bool IsUserRegistered(string email)
        {
            var logServiceUrl = string.Format(ApiConstants.LoginServiceEndpoint, _loginServiceURL, email);
            try
            {
                var response = CommonHelper.GetWebResponse<dynamic>(logServiceUrl, "GET", _loginServiceThumbprint, string.Empty, userName: _loginServiceUser, password: _loginServiceKey);
               return response?.Count > 0 ? true : false;
            }
            catch
            {
                return false;
            }
        }

        private static bool LogApiErrors(string requestUrl, string method, string requestPayload, string responseStatus, string responsePayload)
        {
            try
            {
                new LogApiCallDataAccess().LogApiErrors(requestUrl, method, requestPayload, responseStatus, responsePayload);
                return true;
            }
            catch
            {
                return false;
            }
        }

    }
}
