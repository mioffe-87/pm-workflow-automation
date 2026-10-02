using Csp.Api.HelperClasses;
using Csp.Api.Model;
using Csp.Api.Model.Flex;
using Csp.Api.Services.Contracts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Logic
{
    public class FirewallFlexApiLogic : IFirewallFlexApiLogic
    {
        private readonly IApplicationSettings _applicationSettings;
        public FirewallFlexApiLogic(IApplicationSettings applicationSettings)
        {
            _applicationSettings = applicationSettings;
        }

        public FileInfo DownloadAirgapLicense(long supportAccountId, long userAccountId, string authCode, bool withPanorama)
        {
            dynamic result = null;
            try
            {
                var method = HttpMethod.Get.Method;
                
                var baseUrl = string.Format(withPanorama ? ApiConstants.AirgapLicensesWithPanoramaEndpoint : ApiConstants.AirgapLicensesWithoutPanoramaEndpoint, _applicationSettings.EntitlementServiceBffHost, supportAccountId, userAccountId, authCode);
                result = CommonHelper.DownloadFile(baseUrl, method, _applicationSettings.EntitlementBffThumbprint, string.Empty, true);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }

        public dynamic GetCNFirewallSDevices(FlexDeviceRequest flexDeviceRequest, long userAccountId, long supportAccountId, string type, bool export = false, string subType = null)
        {
            dynamic result = null;
            try
            {
                var method = HttpMethod.Get.Method;
                if (flexDeviceRequest.requestParameters.PaginationCount == 0)
                    flexDeviceRequest.requestParameters.PaginationCount = 10;
                var baseUrl = string.Format(ApiConstants.CNSeriesFWFlexDevicesEndpoint, _applicationSettings.EntitlementServiceBffHost,userAccountId,supportAccountId,type,export,
                              flexDeviceRequest.requestParameters.SearchField, flexDeviceRequest.requestParameters.SearchValue, flexDeviceRequest.requestParameters.PaginationOffset, flexDeviceRequest.requestParameters.PaginationCount, flexDeviceRequest.requestParameters.Sort, subType);
                if (!export)
                    result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.EntitlementBffThumbprint, string.Empty, true);
                else
                    result = CommonHelper.ProcessWebRequest(baseUrl, method, _applicationSettings.EntitlementBffThumbprint, string.Empty, true);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }
   
        public dynamic CreateFirewallFlexDeploymentProfile(FirewallFlexDeploymentProfile firewallFlexDeploymentProfile)
        {
            dynamic result = null;
            try
            {
                var method = HttpMethod.Post.Method;
                var jsonPayload = JsonConvert.SerializeObject(firewallFlexDeploymentProfile);
                var baseUrl = string.Format(ApiConstants.FirewallFlexDeploymentEndpoint, _applicationSettings.EntitlementServiceBffHost);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.EntitlementBffThumbprint, jsonPayload, true);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }

        public dynamic UpdateFirewallFlexDeploymentProfile(FirewallFlexDeploymentProfile firewallFlexDeploymentProfile)
        {
            dynamic result = null;
            try
            {
                var method = new HttpMethod("PATCH").Method;
                var jsonPayload = JsonConvert.SerializeObject(firewallFlexDeploymentProfile);
                var baseUrl = string.Format(ApiConstants.FirewallFlexDeploymentEndpoint, _applicationSettings.EntitlementServiceBffHost);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.EntitlementBffThumbprint, jsonPayload, true);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }

        public dynamic GetEstimatedCost(TokenCalculatorRequest tokenCalculatorRequest, long supportAccountId, long userAccountId)
        {
            dynamic result = null;
            try
            {
                var method = HttpMethod.Post.Method;
                var jsonPayload = JsonConvert.SerializeObject(tokenCalculatorRequest);
                var baseUrl = string.Format(ApiConstants.EstimatedCreditsCostEndpoint, _applicationSettings.EntitlementServiceBffHost, supportAccountId,
                    userAccountId);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.EntitlementBffThumbprint, jsonPayload, true);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }

        public dynamic RetryFailedDeploymentProfile(long deploymentProfileId)
        {
            dynamic result = null;
            try
            {             
                var baseUrl = string.Format(ApiConstants.RetryFailedDeploymentProfileEndpoint, _applicationSettings.EntitlementServiceBffHost, deploymentProfileId);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, new HttpMethod("PATCH").Method, _applicationSettings.EntitlementBffThumbprint, string.Empty, true);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }
    }
}
