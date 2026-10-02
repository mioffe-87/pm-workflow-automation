using Csp.Api.Services.Contracts;
using System;
using Csp.Api.Model;
using Csp.Api.HelperClasses;
using System.Net;
using Newtonsoft.Json;
using System.Net.Http;
using Csp.Api.Model.Enums;
using System.Configuration;
using System.Net.Mail;
using Csp.Api.Security;
using System.Threading.Tasks;
using Csp.Api.DataAccess.HelperClasses;

namespace Csp.Api.Logic
{
    public class SupportAccountUserLogic : ISupportAccountUserLogic
    {
        private readonly IApplicationSettings _applicationSettings;
        private readonly ISecurityRepository _securityRepository;
        private readonly IUserAccountApiLogic _userAccountApiLogic;

        public SupportAccountUserLogic(IApplicationSettings applicationSettings, ISecurityRepository securityRepository, IUserAccountApiLogic userAccountApiLogic)
        {
            _applicationSettings = applicationSettings;
            _securityRepository = securityRepository;
            _userAccountApiLogic = userAccountApiLogic;
        }

        public UserProfileModel GetUserData(string email)
        {
            try
            {
                string encodedEmail = System.Web.HttpUtility.UrlEncode(email);
                string baseUrl = string.Format(ApiConstants.UserAccountDataPath, _applicationSettings.PortalBffHost, encodedEmail);
                var response = CommonHelper.ProcessWebRequest<UserProfileModel>(baseUrl, HttpMethod.Get.Method, _applicationSettings.PortalBffCertThumbPrint, string.Empty);
                return response.ResponseObject;
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return null;
            }
        }

        public CommonApiResponse<dynamic> GetOktaSSOData(string email)
        {
            dynamic response = null;
            try
            {
                string baseUrl = string.Format(ApiConstants.OktaSSODataPath, _applicationSettings.PortalBffHost, email);
                response = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, HttpMethod.Get.Method, _applicationSettings.PortalBffCertThumbPrint, string.Empty);
            }
            catch (WebException ex)
            {
                response = ExceptionHelper.GetWebException(ex);
            }
            return response;
        }

        public CommonApiResponse<dynamic> GetCspMembershipData(string email, long supportAccountId)
        {
            dynamic response = null;
            string baseUrl = string.Format(ApiConstants.CspMembershipPath, _applicationSettings.PortalBffHost, supportAccountId, email);
            response = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, HttpMethod.Get.Method, _applicationSettings.PortalBffCertThumbPrint, string.Empty);
            return response;
        }

        public CommonApiResponse<dynamic> HandleStatus(string email, string status)
        {
            dynamic response = null;
            try
            {
                string baseUrl = string.Format(ApiConstants.OktaPasswordResetPath, _applicationSettings.PortalBffHost, System.Web.HttpUtility.UrlEncode(email), status);
                response = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, HttpMethod.Post.Method, _applicationSettings.PortalBffCertThumbPrint, string.Empty);
            }
            catch (WebException ex)
            {
                response = ExceptionHelper.GetWebException(ex);
            }
            return response;
        }

        public EmailSubscriptions UserSubscriptions(long userAccountId, string crudType, EmailSubscriptions emailSubscriptions = null)
        {
            string method = string.Empty;
            string jsonPayload = string.Empty;

            if (crudType == ManageUserAction.Get.ToDescription())
            {
                method = HttpMethod.Get.Method;
            }
            else if (crudType == ManageUserAction.Update.ToDescription())
            {
                method = HttpMethod.Post.Method;
                jsonPayload = JsonConvert.SerializeObject(emailSubscriptions);
            }

            string baseUrl = string.Format(ApiConstants.UserAccountEmailPreferencesPath, _applicationSettings.PortalBffHost, userAccountId);

            return CommonHelper.ProcessWebRequest<EmailSubscriptions>(baseUrl, method, _applicationSettings.PortalBffCertThumbPrint, jsonPayload).ResponseObject;
        }

        public CommonApiResponse<dynamic> SupportAccountUsers(long supportAccountId, long userAccountId, string action, SupportAccountUser supportAccountUser, bool extension = false)
        {
            string baseUrl = string.Empty;
            string method = string.Empty;
            string jsonPayload = string.Empty;
            dynamic result = null;

            try
            {
                if (action == ManageUserAction.Get.ToDescription())
                {
                    method = HttpMethod.Get.Method;
                    if (!string.IsNullOrEmpty(supportAccountUser.RequestParameters.Sort) && !ValidateSorting(supportAccountUser.RequestParameters.Sort))
                    {
                        supportAccountUser.RequestParameters.Sort = string.Empty;
                    }
                    if (string.IsNullOrEmpty(supportAccountUser.RequestParameters.SearchField))
                    {
                        baseUrl = string.Format(ApiConstants.SupportUserAccountGetDataPathWithoutSearch, _applicationSettings.PortalBffHost, supportAccountId,
                            supportAccountUser.RequestParameters.Page, supportAccountUser.RequestParameters.PageSize,
                            supportAccountUser.RequestParameters.Sort, supportAccountUser.RequestParameters.Status, userAccountId, extension);
                    }
                    else
                    {
                        if (!SearchHelper.ValidateSearchField(supportAccountUser.RequestParameters.SearchField))
                        {
                            supportAccountUser.RequestParameters.SearchField = string.Empty;
                            supportAccountUser.RequestParameters.SearchValue = string.Empty;
                        }
                        baseUrl = string.Format(ApiConstants.SupportUserAccountGetDataPathWithSearch, _applicationSettings.PortalBffHost, supportAccountId,
                            supportAccountUser.RequestParameters.SearchField, System.Web.HttpUtility.UrlEncode(supportAccountUser.RequestParameters.SearchValue), supportAccountUser.RequestParameters.Page,
                            supportAccountUser.RequestParameters.PageSize, supportAccountUser.RequestParameters.Sort, supportAccountUser.RequestParameters.Status, userAccountId, extension);
                    }
                    if (!string.IsNullOrEmpty(supportAccountUser.RequestParameters.Status))
                    {

                        if (supportAccountUser.RequestParameters.Status.ToLower() == MembershipStatus.Pending.ToDescription().ToLower())
                        {
                            baseUrl = string.Format(ApiConstants.SupportUserAccountGetPendingDataPath, baseUrl, userAccountId);

                        }
                    }
                }
                else if (action == ManageUserAction.Create.ToDescription())
                {
                    method = HttpMethod.Post.Method;
                    jsonPayload = JsonConvert.SerializeObject(supportAccountUser);
                    baseUrl = string.Format(ApiConstants.SupportUserAccountAddDataPath, _applicationSettings.PortalBffHost);
                }
                else if (action == ManageUserAction.Update.ToDescription())
                {
                    if (extension)
                    {
                        return ManageUsersMembershipExtensions(supportAccountUser.MembershipExtensionParameters);
                    }
                    method = new HttpMethod("PATCH").Method;
                    jsonPayload = JsonConvert.SerializeObject(supportAccountUser);
                    baseUrl = string.Format(ApiConstants.SupportUserAccountUpdateDataPath, _applicationSettings.PortalBffHost);
                }
                else if (action == ManageUserAction.Delete.ToDescription())
                {
                    method = HttpMethod.Delete.Method;
                    baseUrl = string.Format(ApiConstants.SupportUserAccountDeleteDataPath, _applicationSettings.PortalBffHost, supportAccountUser.MembershipId, supportAccountUser.SupportAccountId, supportAccountUser.UserAccountId);
                }

                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.PortalBffCertThumbPrint, jsonPayload);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return result;
        }


        public CommonApiResponse<dynamic> SupportAccountUserPrivileges(long supportAccountId, long userAccountId)
        {
            dynamic result = null;

            try
            {
                string jsonPayload = string.Empty;
                string method = HttpMethod.Get.Method;
                string baseUrl = string.Format(ApiConstants.UserAccountGetPrivileges, _applicationSettings.PortalBffHost, supportAccountId, userAccountId);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.PortalBffCertThumbPrint, jsonPayload);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw new Exception(ex.Message);
            }
            return result;
        }

        public CommonApiResponse<dynamic> GetAssignableRoles(long supportAccountId, long userAccountId)
        {
            dynamic result = null;

            try
            {
                string baseUrl = string.Format(ApiConstants.AssignableRolesEndpointUrl, _applicationSettings.PortalBffHost, userAccountId, supportAccountId);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, HttpMethod.Get.Method, _applicationSettings.PortalBffCertThumbPrint, string.Empty);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw new Exception(ex.Message);
            }
            return result;
        }

        public CommonApiResponse<dynamic> GetSiteLicense(long supportAccountId)
        {
            dynamic result = null;

            try
            {
                string baseUrl = string.Format(ApiConstants.SiteLicenseEndpoint, _applicationSettings.LicenseApi, supportAccountId);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, HttpMethod.Get.Method, _applicationSettings.LicenseApiThumbprint, string.Empty);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw new Exception(ex.Message);
            }
            return result;
        }

        public CommonApiResponse<dynamic> GetCountries()
        {
            dynamic result = null;

            try
            {
                string baseUrl = string.Format(ApiConstants.CountriesEndpoint, _applicationSettings.PortalBffHost);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, HttpMethod.Get.Method, _applicationSettings.PortalBffCertThumbPrint, string.Empty);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw new Exception(ex.Message);
            }
            return result;
        }

        public CommonApiResponse<dynamic> GetRegions(int countryId)
        {
            dynamic result = null;

            try
            {
                string baseUrl = string.Format(ApiConstants.RegionsEndpoint, _applicationSettings.PortalBffHost, countryId);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, HttpMethod.Get.Method, _applicationSettings.PortalBffCertThumbPrint, string.Empty);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw new Exception(ex.Message);
            }
            return result;
        }

        public CommonApiResponse<dynamic> CreateUser(UserDetail userDetail)
        {
            dynamic result = null;

            try
            {
                string baseUrl = string.Format(ApiConstants.CreateUserEndpoint, _applicationSettings.PortalBffHost);
                var jsonPayload = JsonConvert.SerializeObject(userDetail);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, HttpMethod.Post.Method, _applicationSettings.PortalBffCertThumbPrint, jsonPayload);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw new Exception(ex.Message);
            }
            return result;
        }

        public CommonApiResponse<dynamic> UpdateUser(UserDetail userDetail)
        {
            dynamic result = null;

            try
            {
                string baseUrl = string.Format(ApiConstants.CreateUserEndpoint, _applicationSettings.PortalBffHost);
                var jsonPayload = JsonConvert.SerializeObject(userDetail);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, new HttpMethod("PATCH").Method, _applicationSettings.PortalBffCertThumbPrint, jsonPayload);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw new Exception(ex.Message);
            }
            return result;
        }

        #region Account Details Methods 
        public CommonApiResponse<dynamic> GetSupportAccountDetails(long supportAccountId, bool isCspHeaderRequired)
        {
            dynamic result = null;
            try
            {
                var method = HttpMethod.Get.Method;
                var baseUrl = string.Format(ApiConstants.SupportAccountEndpoint, _applicationSettings.PortalBffHost, supportAccountId);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.PortalBffCertThumbPrint, string.Empty, true, isCspHeaderRequired ? ApiConstants.CspApiHeader : null);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }

        public CommonApiResponse<dynamic> UpdateSupportAccountInfo(long supportAccountId, long userAccountId, SupportAccountInfo supportAccountInfo)
        {
            dynamic result = null;
            try
            {
                var patch = new HttpMethod("PATCH");
                var jsonPayload = JsonConvert.SerializeObject(supportAccountInfo);
                var baseUrl = string.Format(ApiConstants.UpdateSupportAccountEndpoint, _applicationSettings.PortalBffHost, supportAccountId, userAccountId);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, patch.Method, _applicationSettings.PortalBffCertThumbPrint, jsonPayload, true, ApiConstants.CspApiHeader);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }

        public CommonApiResponse<dynamic> GetDefaultRoles(long supportAccountId, long userAccountId, bool isCspHeaderRequired)
        {
            dynamic result = null;
            try
            {
                var method = HttpMethod.Get.Method;
                var baseUrl = string.Format(ApiConstants.DefaultRolesEndpoint, _applicationSettings.PortalBffHost, supportAccountId, userAccountId);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.PortalBffCertThumbPrint, string.Empty, true, isCspHeaderRequired ? ApiConstants.CspApiHeader : null);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }

        public CommonApiResponse<dynamic> UpdateDefaultRoles(long supportAccountId, long userAccountId, RolesInfo roles)
        {
            dynamic result = null;
            try
            {
                var patch = new HttpMethod("PATCH");
                var jsonPayload = JsonConvert.SerializeObject(roles);
                var baseUrl = string.Format(ApiConstants.DefaultRolesEndpoint, _applicationSettings.PortalBffHost, supportAccountId, userAccountId);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, patch.Method, _applicationSettings.PortalBffCertThumbPrint, jsonPayload, true, ApiConstants.CspApiHeader);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }

        public CommonApiResponse<dynamic> GetSelfRegistrationDetails(long supportAccountId, long userAccountId)
        {
            dynamic result = null;
            try
            {
                var method = HttpMethod.Get.Method;
                var baseUrl = string.Format(ApiConstants.GetSelfRegistrationEndpoint, _applicationSettings.PortalBffHost, supportAccountId, userAccountId, ApiConstants.CspRegisterAppId);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.PortalBffCertThumbPrint, string.Empty);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }

        public CommonApiResponse<dynamic> UpdateEmailDomainType(long supportAccountId, UpdateEmailDomain emailDomainInfo)
        {
            dynamic result = null;
            try
            {
                var patch = new HttpMethod("PATCH");
                var jsonPayload = JsonConvert.SerializeObject(emailDomainInfo);
                var baseUrl = string.Format(ApiConstants.UpdateEmailDomainType, _applicationSettings.PortalBffHost, supportAccountId);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, patch.Method, _applicationSettings.PortalBffCertThumbPrint, jsonPayload);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }

        public CommonApiResponse<dynamic> CreateSelfRegistrationDetails(SelfRegistration info)
        {
            dynamic result = null;
            try
            {
                var method = HttpMethod.Post.Method;
                var jsonPayload = JsonConvert.SerializeObject(info);
                var baseUrl = string.Format(ApiConstants.CreateUpdateSelfRegistrationEndpoint, _applicationSettings.PortalBffHost);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.PortalBffCertThumbPrint, jsonPayload);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }
        public CommonApiResponse<dynamic> UpdateSelfRegistrationDetails(UpdateSelfRegistration info)
        {
            dynamic result = null;
            try
            {
                var patch = new HttpMethod("PATCH");
                var jsonPayload = JsonConvert.SerializeObject(info);
                var baseUrl = string.Format(ApiConstants.CreateUpdateSelfRegistrationEndpoint, _applicationSettings.PortalBffHost);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, patch.Method, _applicationSettings.PortalBffCertThumbPrint, jsonPayload);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }
        public CommonApiResponse<dynamic> GetAscInformation(long supportAccountId)
        {
            dynamic result = null;
            try
            {
                var method = HttpMethod.Get.Method;
                var baseUrl = string.Format(ApiConstants.AscInfoEndpoint, _applicationSettings.PortalBffHost, supportAccountId);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.PortalBffCertThumbPrint, string.Empty);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }

        public CommonApiResponse<dynamic> UpdateAscInformation(long supportAccountId, UpdateAscInformation ascInfo)
        {
            dynamic result = null;
            try
            {
                var method = HttpMethod.Put.Method;
                var jsonPayload = JsonConvert.SerializeObject(ascInfo);
                var baseUrl = string.Format(ApiConstants.AscInfoEndpoint, _applicationSettings.PortalBffHost, supportAccountId);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.PortalBffCertThumbPrint, jsonPayload);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }
        public CommonApiResponse<dynamic> ConvertEvalAccount(EvalAccountDetails info)
        {
            dynamic result = null;
            try
            {
                var method = HttpMethod.Post.Method;
                var jsonPayload = JsonConvert.SerializeObject(info);
                var baseUrl = string.Format(ApiConstants.ConvertEvalAccountEndpoint, _applicationSettings.ReportingBffHost);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.ReportingBffCertThumbPrint, jsonPayload);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }

        public SSOThirdPartyInformation GetSSOInformation(long userAccountId, long supportAccountId, string email)
        {
            SSOThirdPartyInformation ssoThirdPartyInformation = new SSOThirdPartyInformation();
            try
            {
                ssoThirdPartyInformation.IsConfigured = false;
                ssoThirdPartyInformation.ThirdPartySsoConfigUrl = "";
                ssoThirdPartyInformation.ShowUrl = false;

                bool isDomainAdmin = _securityRepository.HasRole(supportAccountId, userAccountId, RoleNames.DomainAdministrator);
                if (isDomainAdmin)
                {
                    ssoThirdPartyInformation.domain = new MailAddress(email).Host;
                    ssoThirdPartyInformation.ShowUrl = true;
                    ssoThirdPartyInformation.ThirdPartySsoConfigUrl = string.Format(ConfigurationManager.AppSettings["ThirdPartySsoConfigUrl"], supportAccountId);
                }
                else
                {
                    ssoThirdPartyInformation.Message = "user is not a domain administrator";
                }

            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw new Exception(ex.Message);
            }

            return ssoThirdPartyInformation;
        }
        #endregion

        #region self-registration

        public CommonApiResponse<dynamic> ValidateCaptcha(dynamic request)
        {
            dynamic result = null;
            try
            {
                var method = HttpMethod.Post.Method;
                var jsonPayload = JsonConvert.SerializeObject(request);
                var baseUrl = string.Format(ApiConstants.ValidateCaptcha, _applicationSettings.PortalBffHost);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.PortalBffCertThumbPrint, jsonPayload);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }

        public CommonApiResponse<dynamic> ValidatePasswordResetCaptcha(dynamic request)
        {
            dynamic result = null;
            try
            {
                var method = HttpMethod.Post.Method;
                var jsonPayload = JsonConvert.SerializeObject(request);
                var baseUrl = string.Format(ApiConstants.ValidatePasswordResetCaptcha, _applicationSettings.PortalBffHost);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.PortalBffCertThumbPrint, jsonPayload);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }


        public CommonApiResponse<dynamic> ValidateEmail(dynamic request)
        {
            dynamic result = null;
            try
            {
                string email = request?.email ?? string.Empty;
                string otp = request?.otp ?? string.Empty;

                var otpValidationResult = ValidateOtp(email, otp);
                if (otpValidationResult != null)
                {
                    return otpValidationResult;
                }

                var method = HttpMethod.Post.Method;
                var jsonPayload = JsonConvert.SerializeObject(request);
                var baseUrl = string.Format(ApiConstants.ValidateEmail, _applicationSettings.PortalBffHost);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.PortalBffCertThumbPrint, jsonPayload);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }

        public CommonApiResponse<dynamic> ValidateSelfRegistrationCode(dynamic request)
        {
            dynamic result = null;
            try
            {
                string email = request?.email ?? string.Empty;
                string otp = request?.otp ?? string.Empty;

                var otpValidationResult = ValidateOtp(email, otp);
                if (otpValidationResult != null)
                {
                    return otpValidationResult;
                }

                var method = HttpMethod.Post.Method;
                var jsonPayload = JsonConvert.SerializeObject(request);
                var baseUrl = string.Format(ApiConstants.ValidateSelfRegistrationCode, _applicationSettings.PortalBffHost);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.PortalBffCertThumbPrint, jsonPayload);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }

        public CommonApiResponse<dynamic> ValidateProductSku(dynamic request)
        {
            dynamic result = null;
            try
            {
                string email = request?.email ?? string.Empty;
                string otp = request?.otp ?? string.Empty;

                var otpValidationResult = ValidateOtp(email, otp);
                if (otpValidationResult != null)
                {
                    return otpValidationResult;
                }

                var method = HttpMethod.Post.Method;
                var jsonPayload = JsonConvert.SerializeObject(request);
                var baseUrl = string.Format(ApiConstants.ValidateProductSku, _applicationSettings.PortalBffHost);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.PortalBffCertThumbPrint, jsonPayload);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }

        public CommonApiResponse<dynamic> ValidateClaisec(dynamic request)
        {
            dynamic result = null;
            try
            {
                var method = HttpMethod.Post.Method;
                var jsonPayload = JsonConvert.SerializeObject(request);
                var baseUrl = string.Format(ApiConstants.ValidateClaisec, _applicationSettings.PortalBffHost);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.PortalBffCertThumbPrint, jsonPayload);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }

        public CommonApiResponse<dynamic> ValidateNetsec(dynamic request)
        {
            dynamic result = null;
            try
            {
                var method = HttpMethod.Post.Method;
                var jsonPayload = JsonConvert.SerializeObject(request);
                var baseUrl = string.Format(ApiConstants.ValidateNetsec, _applicationSettings.PortalBffHost);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.PortalBffCertThumbPrint, jsonPayload);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }

        public CommonApiResponse<dynamic> ValidatePublicDomain(long supportAccountId, string email)
        {
            dynamic result = null;
            try
            {
                var method = HttpMethod.Get.Method;
                var baseUrl = string.Format(ApiConstants.ValidatePublicDomain, _applicationSettings.PortalBffHost,
                    supportAccountId, email);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.PortalBffCertThumbPrint, string.Empty);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }

        public CommonApiResponse<dynamic> GetEligibleAccountsByUserEmail(string email)
        {
            dynamic result = null;
            try
            {
                var method = HttpMethod.Get.Method;
                var baseUrl = string.Format(ApiConstants.EligibleAccount, _applicationSettings.PortalBffHost, email);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.PortalBffCertThumbPrint, string.Empty);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }
        #endregion

        public CommonApiResponse<dynamic> EnableUser(EnableUser requestDto)
        {
            dynamic result = null;

            try
            {
                string baseUrl = string.Format(ApiConstants.EnableUserStatusEndpoint, _applicationSettings.CspApiBffHost);
                var jsonPayload = JsonConvert.SerializeObject(requestDto);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, new HttpMethod("PATCH").Method, _applicationSettings.CspApiBffCertThumbPrint, jsonPayload);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw new Exception(ex.Message);
            }
            return result;
        }

        public dynamic GetMembershipByRole(long supportAccountId, long roleId, long number, long size, string sort)
        {
            dynamic result = null;

            try
            {
                string baseUrl = string.Format(ApiConstants.MembershipByRoleEndpoint, _applicationSettings.PortalBffHost, supportAccountId, roleId, number, size, sort);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, HttpMethod.Get.Method, _applicationSettings.PortalBffCertThumbPrint, null);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw new Exception(ex.Message);
            }
            return result;
        }

        public dynamic NotifySuperUser(NotifySuperUserRequest request)
        {
            dynamic result = null;

            try
            {
                string baseUrl = string.Format(ApiConstants.NotifySuperUserEndpoint, _applicationSettings.PortalBffHost);
                var jsonPayload = JsonConvert.SerializeObject(request);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, HttpMethod.Post.Method, _applicationSettings.PortalBffCertThumbPrint, jsonPayload);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw new Exception(ex.Message);
            }
            return result;
        }


        private dynamic ManageUsersMembershipExtensions(MembershipExtensionRequest requestDto)
        {
            dynamic result = null;

            try
            {
                string baseUrl = string.Format(ApiConstants.MembershipExtensionEndpoint, _applicationSettings.PortalBffHost);
                var jsonPayload = JsonConvert.SerializeObject(requestDto);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, new HttpMethod("PATCH").Method, _applicationSettings.PortalBffCertThumbPrint, jsonPayload);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw new Exception(ex.Message);
            }
            return result;
        }

        public async Task<bool> ValidateSfdcContact(long supportAccountId, long userAccountId, string email)
        {
            try
            {
                if (email.Contains(ApiConstants.PANW_EMPLOYEE_EMAIL_EXTENSION))
                {
                    return true;
                }
                dynamic request = new
                {
                    supportAccountId = supportAccountId,
                    userAccountId = userAccountId,
                    isCreateContact = true
                };
                var jsonPayload = JsonConvert.SerializeObject(request);
                var method = HttpMethod.Post.Method;
                var baseUrl = string.Format(ApiConstants.VerifySfdcContactEndpoint, _applicationSettings.PortalBffHost);
                await CommonHelper.ProcessWebRequestAsync<dynamic>(baseUrl, method, _applicationSettings.PortalBffCertThumbPrint, jsonPayload);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return false;
            }
            return true;
        }

        private bool ValidateSorting(string sorting)
        {
            string sortField = string.Empty;
            string sortOrder = string.Empty;
            var sorts = sorting.Split(',');
            if (sorts?.Length > 0)
            {
                sortField = sorts[0];
                sortOrder = sorts.Length > 2 ? sorts[2] : sorts.Length > 1 ? sorts[1] : string.Empty;
            }

            return SortHelper.ValidateSorting(sortField, sortOrder);
        }

        private dynamic ValidateOtp(string email, string otp)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(otp))
            {
                return new CommonApiResponse<dynamic>
                {
                    ResponseObject = new
                    {
                        status = 400,
                        error_message = new[] { new { error_message = "Invalid request" } },
                        timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
                    }
                };
            }

            var otpValidationRequest = new SelfRegistraionOtpValidationRequest
            {
                email = email,
                otp = otp
            };
            var otpValidationResult = _userAccountApiLogic.ValidateSelfRegistrationOtp(otpValidationRequest);

            if (otpValidationResult == null)
            {
                return otpValidationResult;
            }

            bool hasStatus = HasProperty(otpValidationResult, "status");
            if (hasStatus && otpValidationResult.status != 200)
            {
                return otpValidationResult;
            }

            bool hasResult = HasProperty(otpValidationResult, "result");
            bool hasIsValid = HasProperty(otpValidationResult, "is_valid");
            bool hasMatchCount = HasProperty(otpValidationResult, "match_count");

            if (!hasResult || !hasIsValid || !hasMatchCount)
            {
                return otpValidationResult;
            }

            if (otpValidationResult.result == null ||
                string.IsNullOrEmpty(otpValidationResult.result?.ToString()) ||
                otpValidationResult.is_valid != true ||
                otpValidationResult.match_count < 0)
            {
                return otpValidationResult;
            }

            return null;
        }

        private bool HasProperty(dynamic obj, string propertyName)
        {
            if (obj == null) return false;

            if (obj is Newtonsoft.Json.Linq.JObject jObject)
            {
                return jObject.ContainsKey(propertyName);
            }

            var type = obj.GetType();
            return type.GetProperty(propertyName) != null;
        }


        public CommonApiResponse<dynamic> UnBlockUser(UnblockUser requestDto)
        {
            dynamic result = null;

            try
            {
                string baseUrl = string.Format(ApiConstants.UnBlockUserStatusEndpoint, _applicationSettings.CspApiBffHost);
                var jsonPayload = JsonConvert.SerializeObject(requestDto);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, new HttpMethod("PATCH").Method, _applicationSettings.CspApiBffCertThumbPrint, jsonPayload);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw new Exception(ex.Message);
            }
            return result;
        }

        #region JWT Token Management


        /// <summary>
        /// Invalidates all JWT tokens for the user by calling csp-auth logout endpoint.
        /// This ensures that any active JWT sessions are terminated when the user logs out.
        /// </summary>
        /// <param name="userAccountId">The user account ID whose JWT tokens should be invalidated</param>
        public void InvalidateJwtTokens(long userAccountId)
        {
            dynamic result = null;

            try
            {
                string baseUrl = string.Format(ApiConstants.InvalidateJwtTokensUrl, _applicationSettings.CspAuthHost, userAccountId);

                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, new HttpMethod("DELETE").Method, _applicationSettings.CspAuthCertThumbprint, null);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw new Exception(ex.Message);
            }

        }


        #endregion
    }
}
