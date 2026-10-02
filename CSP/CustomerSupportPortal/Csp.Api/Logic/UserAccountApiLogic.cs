using System.Net;
using System.Net.Http;
using Csp.Api.HelperClasses;
using Csp.Api.Model;
using Csp.Api.Services.Contracts;
using Newtonsoft.Json;
using System;
namespace Csp.Api.Logic
{
    public class UserAccountApiLogic : IUserAccountApiLogic
    {
        private readonly IApplicationSettings _applicationSettings;

        public UserAccountApiLogic(IApplicationSettings applicationSettings) {
            _applicationSettings = applicationSettings;
        }

        public string GenerateSelfRegistrationOtp(SelfRegistraionOtpRequest generateOtpRequest)
        {
            string result;
            try
            {
                var method = HttpMethod.Post.Method;
                var baseUrl = string.Format(ApiConstants.GenerateSelfRegistrationOtpEndpoint, _applicationSettings.PortalBffHost);
                var jsonPayload = JsonConvert.SerializeObject(generateOtpRequest);
                result = CommonHelper.GetWebResponse<string>(baseUrl, method, _applicationSettings.PortalBffCertThumbPrint, jsonPayload, true);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }

        public dynamic GeneratePasswordResetOtp(SelfRegistraionOtpRequest generateOtpRequest)
        {
            dynamic result = null;
            try
            {
                var method = HttpMethod.Post.Method;
                var baseUrl = string.Format(ApiConstants.GeneratePasswordResetOtpEndpoint, _applicationSettings.PortalBffHost, System.Web.HttpUtility.UrlEncode(generateOtpRequest.email));
                var jsonPayload = JsonConvert.SerializeObject(generateOtpRequest);
                result = CommonHelper.GetWebResponse<dynamic>(baseUrl, method, _applicationSettings.PortalBffCertThumbPrint, jsonPayload, true);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }

        public dynamic IsIdpEnabled(string domain)
        {
            dynamic result = null;
            try
            {
                var method = HttpMethod.Get.Method;
                var baseUrl = string.Format(ApiConstants.IsIdpEnabledEndpoint, _applicationSettings.PortalBffHost, domain);
                result = CommonHelper.GetWebResponse<dynamic>(baseUrl, method, _applicationSettings.PortalBffCertThumbPrint, "", true);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }

        public dynamic ContactSupport(dynamic model)
        {
            dynamic result = null;
            try
            {
                var method = HttpMethod.Post.Method;
                var baseUrl = string.Format(ApiConstants.ContactSupportEndpoint, _applicationSettings.PortalBffHost);
                var jsonPayload = JsonConvert.SerializeObject(model);
                result = CommonHelper.GetWebResponse<dynamic>(baseUrl, method, _applicationSettings.PortalBffCertThumbPrint, jsonPayload, true);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }

        public dynamic ValidateSelfRegistrationOtp(SelfRegistraionOtpValidationRequest validateOtpRequest)
        {
            dynamic result = null;
            try
            {
                var method = HttpMethod.Post.Method;
                var baseUrl = string.Format(ApiConstants.ValidateSelfRegistrationOtpEndpoint,
                    _applicationSettings.PortalBffHost);
                var jsonPayload = JsonConvert.SerializeObject(validateOtpRequest);
                result = CommonHelper.GetWebResponse<dynamic>(baseUrl, method,
                    _applicationSettings.PortalBffCertThumbPrint, jsonPayload, true);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }

        public dynamic RegisterClaisec(dynamic model)
        {
            try
            {
                string method = HttpMethod.Post.ToString();
                var baseUrl = string.Format(ApiConstants.UserRegistrationClaisecEndpoint, _applicationSettings.PortalBffHost);
                var jsonPayload = JsonConvert.SerializeObject(model);
                return CommonHelper.GetWebResponse<dynamic>(baseUrl, method, _applicationSettings.PortalBffCertThumbPrint, jsonPayload, true);
            }
            catch (WebException ex)
            {
                return ExceptionHelper.GetWebException(ex);
            }
        }

        public dynamic RegisterNetsec(dynamic model)
        {
            try
            {
                string method = HttpMethod.Post.ToString();
                var baseUrl = string.Format(ApiConstants.UserRegistrationNetsecEndpoint, _applicationSettings.PortalBffHost);
                var jsonPayload = JsonConvert.SerializeObject(model);
                return CommonHelper.GetWebResponse<dynamic>(baseUrl, method, _applicationSettings.PortalBffCertThumbPrint, jsonPayload, true);
            }
            catch (WebException ex)
            {
                return ExceptionHelper.GetWebException(ex);
            }
        }

        public dynamic RegisterByCode(dynamic model)
        {
            try
            {
                string method = HttpMethod.Post.ToString();
                var baseUrl = string.Format(ApiConstants.UserRegistrationByCodeEndpoint, _applicationSettings.PortalBffHost);
                var jsonPayload = JsonConvert.SerializeObject(model);
                return CommonHelper.GetWebResponse<dynamic>(baseUrl, method, _applicationSettings.PortalBffCertThumbPrint, jsonPayload, true);
            }
            catch (WebException ex)
            {
                return ExceptionHelper.GetWebException(ex);
            }
        }

        public CommonApiResponse<dynamic> GetAllCloudServices()
        {
            dynamic result = null;
            try
            {
                var method = HttpMethod.Get.Method;
                var baseUrl = string.Format(ApiConstants.CloudServicesEndpoint, _applicationSettings.ReportingBffHost);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.ReportingBffCertThumbPrint, string.Empty, true);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }
        public dynamic RemoveBounceListEmails(EmailList model)
        {
            dynamic result = null;
            try
            {
                var method = HttpMethod.Post.Method;
                var baseUrl = string.Format(ApiConstants.RemoveBounceListEmailsEndpoint, _applicationSettings.PortalBffHost);
                var jsonPayload = JsonConvert.SerializeObject(model);
                result = CommonHelper.GetWebResponse<dynamic>(baseUrl, method, _applicationSettings.PortalBffCertThumbPrint, jsonPayload, true);
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
    }


}
