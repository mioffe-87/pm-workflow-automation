using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Csp.Api.HelperClasses;
using Csp.Api.Model;
using Csp.Api.Model.Enums;
using Csp.Api.Services;
using Csp.Api.Services.Contracts;
using ExpressiveAnnotations.Analysis;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Csp.Api.DataAccess;


namespace Csp.Api.Logic
{
    public class MfaLogic : IMfaLogic
    {
        #region Constants

        private static string OrgAlias = ConfigurationManager.AppSettings["OrgAlias"];
        private static string Base64Token = ConfigurationManager.AppSettings["Base64Token"];
        private static string Token = ConfigurationManager.AppSettings["Token"];
        private static string ApiVersion = ConfigurationManager.AppSettings["ApiVersion"];
        private static string BaseUrl = ConfigurationManager.AppSettings["PingIdBaseUrl"];

        private const string Org_Alias_Param = "org_alias";
        private const string Token_Param = "token";
        private const string Locale_Param = "locale";
        private const string OrgAlias_Param = "orgAlias";
        private const string SecretKey_Param = "secretKey";
        private const string Timestamp_Param = "timestamp";
        private const string Version_Param = "version";

        private const string Hashing_Algo = "HS256";
        private const string Locale = "en";

        private const string ReqHeader = "reqHeader";
        private const string ReqBody = "reqBody";

        private const string AddUser = "adduser";
        private const string DeleteUser = "deleteuser";
        private const string OfflinePairing = "offlinepairing";

        private const long SystemUser = 1;
        private const string User_Exists = "already exists";

        #endregion

        private readonly IMfaDataAccess _mfaDataAccess;
        private readonly ISecurityRepository _securityRepository;
        private readonly ILoginServiceLogic _loginServiceLogic;

        public MfaLogic(IMfaDataAccess mfaDataAccess, ISecurityRepository securityRepository, ILoginServiceLogic loginServiceLogic)
        {
            _mfaDataAccess = mfaDataAccess;
            _securityRepository = securityRepository;
            _loginServiceLogic = loginServiceLogic;
        }

        /// <summary>
        /// Enables 2 Factor Authentication at the account level for all the members and 
        /// queues all the user registrations to be done with Ping Identity
        /// </summary>
        /// <param name="supportAccountId"></param>
        /// <param name="enabledByUserId"></param>
        /// <param name="isRequired"></param>
        /// <param name="accountName"></param>
        /// <param name="updatedByUser"></param>
        /// <returns></returns>
        public bool EnableMfaForAccount(long supportAccountId, long enabledByUserId, bool isRequired, string accountName, string updatedByUser)
        {
            try
            {
                var userEmails = _mfaDataAccess.EnableMfaForAccount(supportAccountId, enabledByUserId, isRequired);

                //Notify users when 2FA is disabled at account level
                if (!isRequired && userEmails.Any())
                {
                    EmailHelper.SendMfaDisableEmail(userEmails, accountName, updatedByUser);
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Process all the pending user registrations for MFA
        /// </summary>
        /// <returns></returns>
        public void ProcessPendingMfaUserRegistrations()
        {
            var users = _mfaDataAccess.GetPendingMfaRegistrations();

            //Call Ping Identity APIs to register each user and enable offline pairing
            foreach (var user in users)
            {
                EnableMfa(user, SystemUser);
            }
        }

        /// <summary>
        /// Enable MFA and register user's email in Ping Identity
        /// </summary>
        /// <param name="user"></param>
        /// <param name="enabledByUserId"></param>
        /// <param name="mfaAuthType"></param>
        /// <param name="secret"></param>
        /// <param name="unEnrollGoogleAuth"></param>
        /// <returns></returns>
        public bool EnableMfa(UserAccount user, long enabledByUserId, MultiFactorAuthType mfaAuthType = MultiFactorAuthType.Email,
                                string secret = null, bool unEnrollGoogleAuth = false)
        {
            var mfaRegistrationModel = new MfaRegistrationModel { UserAccountId = user.Id, UserEmail = user.Email, MultiFactorAuthType = mfaAuthType, UpdatedByUserAccountId = enabledByUserId };
            var enabledByAdmin = user.Id != enabledByUserId;
            var responseMessage = string.Empty;

            try
            {
                var isRegistered = false;
                var sendRegistrationEmail = false;

                if (mfaAuthType.Equals(MultiFactorAuthType.Email))
                {
                    //User is already enrolled in MFA via email
                    if (user.IsMfaEnabled && user.MultiFactorAuthType.Equals(MultiFactorAuthType.Email))
                        return true;

                    #region Email Registration
                    //Add user in Ping Identity
                    var addUserReqBody = new Dictionary<string, string>
                    {
                        {"activateUser", "false"},
                        {"fName", user.FirstName},
                        {"lname", user.LastName},
                        {"email", user.Email},
                        {"username", user.Email},
                        {"role", "REGULAR"}
                    };
                    var response = CallPingApi(string.Format(BaseUrl, AddUser), user.Email, addUserReqBody);
                    if (response.Status.Equals(MfaRegistrationStatus.Success))
                    {
                        //Register the user email for 2 Factor Authentication
                        var offlinePairingReqBody = new Dictionary<string, string>
                        {
                            {"username", user.Email},
                            {"type", "EMAIL"},
                            {"pairingData", user.Email}
                        };
                        response = CallPingApi(string.Format(BaseUrl, OfflinePairing), user.Email, offlinePairingReqBody);
                    }

                    if (response.Status.Equals(MfaRegistrationStatus.Failure) && response.ResponseBody.ErrorMsg.Contains(User_Exists))
                    {
                        response.Status = MfaRegistrationStatus.Success;
                    }

                    mfaRegistrationModel.RegistrationStatus = response.Status;
                    mfaRegistrationModel.StatusCode = response.ResponseBody.ErrorId;
                    mfaRegistrationModel.StatusMessage = response.ResponseBody.ErrorMsg;
                    mfaRegistrationModel.UnEnrollGoogleAuth = unEnrollGoogleAuth;

                    #endregion
                }
                else if (mfaAuthType.Equals(MultiFactorAuthType.GoogleAuthenticator))
                {
                    if (user.IsMfaEnabled && user.MultiFactorAuthType.Equals(MultiFactorAuthType.GoogleAuthenticator) && string.IsNullOrEmpty(secret))
                        return true;

                    //Enroll in Google Authenticator
                    mfaRegistrationModel.RegistrationStatus = user.IsGoogleAuthEnrolled ? MfaRegistrationStatus.Success : MfaRegistrationStatus.Pending;
                    mfaRegistrationModel.StatusMessage = user.IsGoogleAuthEnrolled ? "Enrolled" : "Pending Enrollment";
                    mfaRegistrationModel.GoogleAuthenticatorSecret = secret;
                }

                //Update user registration
                sendRegistrationEmail = mfaRegistrationModel.RegistrationStatus.Equals(MfaRegistrationStatus.Success);

                var isGoogleAuthEnrolled = _loginServiceLogic.ManageGoogleAuth(ManageUserAction.Get.ToDescription(), out responseMessage, mfaRegistrationModel.UserEmail);

                if (mfaRegistrationModel.MultiFactorAuthType == MultiFactorAuthType.GoogleAuthenticator
                    && mfaRegistrationModel.RegistrationStatus == MfaRegistrationStatus.Success && !string.IsNullOrEmpty(mfaRegistrationModel.GoogleAuthenticatorSecret)
                    && string.IsNullOrEmpty(responseMessage))
                {
                    var googleAuthRequest = new GoogleAuthRequest()
                    {
                        Email = mfaRegistrationModel.UserEmail,
                        SecretKey = mfaRegistrationModel.GoogleAuthenticatorSecret,
                        RequestedBy = string.IsNullOrEmpty(HttpContext.Current.User.Identity.Name) ? GetLoggedInEmail(mfaRegistrationModel.UpdatedByUserAccountId)
                        : HttpContext.Current.User.Identity.Name
                    };
                    var crudType = (!isGoogleAuthEnrolled) ? ManageUserAction.Create.ToDescription() : ManageUserAction.Update.ToDescription();
                    _loginServiceLogic.ManageGoogleAuth(crudType, out responseMessage, googleAuthRequest: googleAuthRequest);
                }

                if (string.IsNullOrEmpty(responseMessage))
                {
                    isRegistered = _mfaDataAccess.UpdateUserRegistration(true, mfaRegistrationModel);
                }

                if (mfaRegistrationModel.UnEnrollGoogleAuth && string.IsNullOrEmpty(responseMessage))
                {
                    var isDisabled = _loginServiceLogic.ManageGoogleAuth(ManageUserAction.Delete.ToDescription(), out responseMessage, mfaRegistrationModel.UserEmail);
                    _mfaDataAccess.LogMfa(mfaRegistrationModel);
                    isRegistered = !isDisabled && string.IsNullOrEmpty(responseMessage);
                }

                if (isRegistered && sendRegistrationEmail)
                {
                    //Send email after MFA registration is complete
                    EmailHelper.SendMfaActivationEmail(user.Email, string.Format("{0} {1}", user.FirstName, user.LastName), mfaAuthType, enabledByAdmin);
                }

                return isRegistered;
            }
            catch (Exception ex)
            {

                //Log the error in MfaUserRegistration table
                if (HttpContext.Current != null)
                {
                    Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                }
                return false;
            }
        }

        /// <summary>
        /// Disable MFA for a user
        /// </summary>
        /// <param name="user"></param>
        /// <param name="supportAccountId"></param>
        /// <param name="disabledByUserAccountId"></param>
        /// <param name="mfaAuthType"></param>
        /// <returns></returns>
        public bool DisableMfa(UserAccount user, long supportAccountId, long disabledByUserAccountId, MultiFactorAuthType mfaAuthType = MultiFactorAuthType.Email)
        {
            var mfaDisabled = false;
            var responseMessage = string.Empty;

            try
            {
                var mfaRegistrationModel = new MfaRegistrationModel { UserAccountId = user.Id, UserEmail = user.Email, MultiFactorAuthType = MultiFactorAuthType.Email };
                if (user.IsMfaEnabled)
                {
                    //Delete user in Ping Identity
                    var deleteUserReqBody = new Dictionary<string, string>
                    {
                        {"userName", user.Email}
                    };
                    var response = CallPingApi(string.Format(BaseUrl, DeleteUser), user.Email, deleteUserReqBody);
                    mfaRegistrationModel.RegistrationStatus = response.Status;
                    mfaRegistrationModel.StatusCode = response.ResponseBody.ErrorId;
                    mfaRegistrationModel.StatusMessage = response.ResponseBody.ErrorMsg;
                    mfaRegistrationModel.IsRegistered = !mfaRegistrationModel.RegistrationStatus.Equals(MfaRegistrationStatus.Success);
                    mfaRegistrationModel.UpdatedByUserAccountId = disabledByUserAccountId;

                    var isGoogleAuthEnrolled = _loginServiceLogic.ManageGoogleAuth(ManageUserAction.Get.ToDescription(), out responseMessage, mfaRegistrationModel.UserEmail);

                    if (isGoogleAuthEnrolled && string.IsNullOrEmpty(responseMessage))
                    {
                        var isDisabled = _loginServiceLogic.ManageGoogleAuth(ManageUserAction.Delete.ToDescription(), out responseMessage, mfaRegistrationModel.UserEmail);
                        mfaDisabled = !isDisabled && string.IsNullOrEmpty(responseMessage);
                        mfaRegistrationModel.RegistrationStatus = 0;
                        mfaRegistrationModel.MultiFactorAuthType = MultiFactorAuthType.None;
                        _mfaDataAccess.LogMfa(mfaRegistrationModel);
                    }

                    //Delete record in MfaUserRegistration 
                    if (string.IsNullOrEmpty(responseMessage))
                    {
                        _mfaDataAccess.UpdateUserRegistration(false, mfaRegistrationModel);
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return mfaDisabled;
        }

        /// <summary>
        /// Checks if MFA is required in any other support account membership for the user
        /// </summary>
        /// <param name="supportAccountId"></param>
        /// <param name="userAccountId"></param>
        /// <returns></returns>
        public bool IsMfaRequiredForAnyMembership(long supportAccountId, long userAccountId)
        {
            var isMfaRequired = _mfaDataAccess.GetUserAccountMfaAggregate(userAccountId, supportAccountId);

            return isMfaRequired;
        }

        /// <summary>
        /// Checks if MFA is required for the given supportAccount
        /// </summary>
        /// <param name="supportAccountId"></param>
        /// <returns></returns>
        public bool IsMfaRequiredForAccount(long supportAccountId)
        {
            var isMfaRequired = _securityRepository.IsAccountMfaEnabled(supportAccountId);

            return isMfaRequired;
        }

        /// <summary>
        /// Checks if MFA is required for the user so he cannot opt out
        /// </summary>
        /// <param name="userEmail"></param>
        /// <returns></returns>
        public bool IsMfaRequiredForUser(string userEmail)
        {
            var isMfaRequired = _mfaDataAccess.IsMfaRequiredforUser(userEmail);

            return isMfaRequired;
        }

        public bool IsGoogleAuthEnrolled(string userEmail)
        {
            string responseMessage = string.Empty;
            return _loginServiceLogic.ManageGoogleAuth(ManageUserAction.Get.ToDescription(), out responseMessage, userEmail);
        }

        #region Ping Identity API calls

        /// <summary>
        /// Call Ping Identity APIs
        /// </summary>
        /// <param name="apiEndPoint"></param>
        /// <param name="requestPayload"></param>
        /// <param name="userEmail"></param>
        /// <returns></returns>
        private PingIdResponsePayload CallPingApi(string apiEndPoint, string userEmail, Dictionary<string, string> requestPayload)
        {
            try
            {
                var uri = new Uri(apiEndPoint);
                var jsonRequest = JsonConvert.SerializeObject(requestPayload);
                var requestToken = GeneratePingIdToken(requestPayload);

                using (var client = new HttpClient { BaseAddress = uri })
                {
                    HttpContent content = new StringContent(requestToken, Encoding.UTF8, "application/json");
                    using (var responseMessage = client.PostAsync(uri, content).Result)
                    {
                        var response = responseMessage.Content.ReadAsStringAsync().Result;

                        var tokenHandler = new JwtSecurityTokenHandler();
                        var jwtToken = tokenHandler.ReadToken(response) as JwtSecurityToken;
                        var jsonResponse = jwtToken != null ? jwtToken.Payload.SerializeToJson() : string.Empty;

                        //Log API call
                        _mfaDataAccess.LogMfaApiCall(userEmail, apiEndPoint, jsonRequest, jsonResponse);

                        if (!string.IsNullOrEmpty(jsonResponse))
                        {
                            var responsePayload = JsonConvert.DeserializeObject<PingIdResponsePayload>(jsonResponse);
                            responsePayload.Status = responseMessage.IsSuccessStatusCode ? MfaRegistrationStatus.Success : MfaRegistrationStatus.Failure;

                            return responsePayload;
                        }

                        return new PingIdResponsePayload
                        {
                            ResponseBody = new PingIdResponse { ErrorId = responseMessage.StatusCode.ToString(), ErrorMsg = responseMessage.ReasonPhrase },
                            Status = MfaRegistrationStatus.Failure
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return new PingIdResponsePayload
                {
                    ResponseBody = new PingIdResponse { ErrorId = HttpStatusCode.BadRequest.ToString(), ErrorMsg = ex.Message },
                    Status = MfaRegistrationStatus.Failure
                };
            }

        }

        /// <summary>
        /// Generate JWT for Ping API authentication
        /// </summary>
        /// <param name="requestPayload"></param>
        /// <returns></returns>
        private static string GeneratePingIdToken(Dictionary<string, string> requestPayload)
        {
            var jwtHeader = new JwtHeader(new SigningCredentials(new SymmetricSecurityKey(Convert.FromBase64String(Base64Token)), Hashing_Algo))
                            { { Org_Alias_Param, OrgAlias }, { Token_Param, Token } };


            var reqHeader = new Dictionary<string, string>
            {
                {Locale_Param, Locale},
                {OrgAlias_Param, OrgAlias},
                {SecretKey_Param, Token},
                {Timestamp_Param, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")},
                {Version_Param, ApiVersion }
            };

            var jwtPayload = new JwtPayload { { ReqHeader, reqHeader }, { ReqBody, requestPayload } };

            SecurityToken jwt = new JwtSecurityToken(jwtHeader, jwtPayload);
            var token = new JwtSecurityTokenHandler().WriteToken(jwt);

            return token;
        }

        /// <summary>
        /// Generate Google Authenticator API token
        /// </summary>
        /// <returns></returns>
        private static string GenerateGoogleAuthApiToken()
        {
            var secret = ConfigurationManager.AppSettings["GoogleAuthApiKey"];
            var issuer = ConfigurationManager.AppSettings["GoogleAuthKeyIssuer"];
            var jwtHeader = new JwtHeader(new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)), Hashing_Algo));
            var jwtPayload = new JwtPayload
                                {
                                    { "jti", "pingidentity.com" },
                                    { "sub", "googleauthtotp" },
                                    { "iss", issuer },
                                    { "aud", issuer },
                                    { "exp", "15162390220" }
                                };
            SecurityToken jwt = new JwtSecurityToken(jwtHeader, jwtPayload);
            var token = new JwtSecurityTokenHandler().WriteToken(jwt);

            return token;
        }

        /// <summary>
        /// Obfuscate the google authenticator secret
        /// </summary>
        /// <param name="secret"></param>
        /// <returns></returns>
        public string ObfuscateSecret(string secret)
        {
            var obfuscatedSecret = string.Empty;

            try
            {
                var googleAuthApi = string.Format(ConfigurationManager.AppSettings["GoogleAuthApiUrl"], secret);
                var requestToken = GenerateGoogleAuthApiToken();

                using (var client = new HttpClient { BaseAddress = new Uri(googleAuthApi) })
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", requestToken);
                    using (var responseMessage = client.GetAsync(new Uri(googleAuthApi)).Result)
                    {
                        var response = responseMessage.Content.ReadAsStringAsync().Result;
                        if (!string.IsNullOrEmpty(response))
                        {
                            var responsePayload =
                                JsonConvert.DeserializeObject<GoogleAuthenticatorResponsePayload>(response);
                            responsePayload.IsSuccess = responseMessage.IsSuccessStatusCode;

                            obfuscatedSecret = responsePayload.EncryptedSecret;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return obfuscatedSecret;
        }

        private string GetLoggedInEmail(long userAccountId)
        {
            UserAccount userAccount = new UserAccount();
            UserAccountDataAccess userAccountDataAccess = new UserAccountDataAccess();

            userAccount = userAccountDataAccess.Read(userAccountId, userAccountId);
            return userAccount.Email;
        }

        #endregion
    }
}
