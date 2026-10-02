using Csp.Api.HelperClasses;
using Csp.Api.Model;
using Csp.Api.Services;
using Csp.Api.Services.Contracts;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;

namespace Csp.Api.Logic
{
    public class OAuthApiLogic : IOAuthApiLogic
    {

        private readonly IApplicationSettings _applicationSettings;

        public OAuthApiLogic(IApplicationSettings applicationSettings)
        {
            _applicationSettings = applicationSettings;
        }


       public CommonApiResponse<dynamic> GetOAuthCreds(long userAccountId, long supportAccountId)
       {
            dynamic result = null;
            try
            {
                var method = HttpMethod.Get.Method;
                var baseUrl = string.Format(ApiConstants.OAuthApiCredsEndpoint, _applicationSettings.PortalBffHost, supportAccountId, userAccountId);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.PortalBffCertThumbPrint, string.Empty, true);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }

       public CommonApiResponse<dynamic> CreateRegenerateOAuthCreds(OAuthApiRequest request)
       {
            dynamic result = null;
            try
            {
                var method = HttpMethod.Post.Method;
                var baseUrl = string.Format(ApiConstants.OAuthApiCredsCreateRegenerateEndpoint, _applicationSettings.PortalBffHost);
                var jsonPayload = JsonConvert.SerializeObject(request);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.PortalBffCertThumbPrint, jsonPayload, true);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }

        public CommonApiResponse<dynamic> GetCustomerDetails(string clientId)
        {
            dynamic result = null;
            try
            {
                var method = HttpMethod.Get.Method;
                var baseUrl = string.Format(ApiConstants.OAuthApiCustomerDetailsEndpoint, _applicationSettings.PortalBffHost, clientId);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.PortalBffCertThumbPrint, string.Empty, true);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;

        }
    }
}
