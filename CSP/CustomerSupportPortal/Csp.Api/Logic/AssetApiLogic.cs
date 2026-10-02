using Csp.Api.HelperClasses;
using Csp.Api.Model;
using Csp.Api.Services.Contracts;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Web;

namespace Csp.Api.Logic
{
    public class AssetApiLogic : IAssetApiLogic
    {
        private readonly IApplicationSettings _applicationSettings;

        public AssetApiLogic(IApplicationSettings applicationSettings)
        {
            _applicationSettings = applicationSettings;
        }

        public CommonApiResponse<dynamic> GetAddOnsAndSubscriptions(long userAccountId, long supportAccountId, string tenantId)
        {
            dynamic result = null;
            try
            {
                var method = HttpMethod.Get.Method;
                var baseUrl = string.Format(ApiConstants.AddOnsEndpoint, _applicationSettings.ReportingBffHost, supportAccountId, tenantId);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.ReportingBffCertThumbPrint, string.Empty, true);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }

        public CommonApiResponse<dynamic> RegisterAsset(BaseRequest request)
        {
            dynamic result = null;
            try
            {
                var method = HttpMethod.Post.Method;
                var jsonPayload = JsonConvert.SerializeObject(request);
                var baseUrl = string.Format(ApiConstants.RegisterSpare, _applicationSettings.ReportingBffHost);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.ReportingBffCertThumbPrint, jsonPayload, true);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }

        public CommonApiResponse<dynamic> InitiateBulkTransfer(DeviceTransferBulkRequest bulkDeviceTransferRequest)
        {
            dynamic result = null;
            try
            {
                var method = HttpMethod.Post.Method;
                var baseUrl = string.Format(ApiConstants.BulkDeviceTransferEndpoint, _applicationSettings.ReportingBffHost);
                var jsonPayload = JsonConvert.SerializeObject(bulkDeviceTransferRequest);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.ReportingBffCertThumbPrint, jsonPayload, true);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }

        public CommonApiResponse<dynamic> BeginTransfer(DeviceTransferPayload deviceTransfer)
        {
            dynamic result = null;
            try
            {
                var method = HttpMethod.Post.Method;
                var baseUrl = string.Format(ApiConstants.AssetTransferEndpoint, _applicationSettings.ReportingBffHost);
                var jsonPayload = JsonConvert.SerializeObject(deviceTransfer);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.ReportingBffCertThumbPrint, jsonPayload, true);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }

        public CommonApiResponse<dynamic> ReviewTransfer(DeviceTransferReviewRequest deviceTransferReview)
        {
            dynamic result = null;
            try
            {
                var method = HttpMethod.Post.Method;
                var baseUrl = string.Format(ApiConstants.AssetTransferReviewEndpoint, _applicationSettings.ReportingBffHost);
                var jsonPayload = JsonConvert.SerializeObject(deviceTransferReview);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.ReportingBffCertThumbPrint, jsonPayload, true);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }

        public CommonApiResponse<dynamic> DownloadAssetData(AssetRequest assetRequest)
        {
            var method = HttpMethod.Post.Method;
            var jsonPayload = JsonConvert.SerializeObject(assetRequest);
            var baseUrl = string.Format(ApiConstants.AssetsDownloadEndpoint, _applicationSettings.ReportingBffHost);
            return CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.ReportingBffCertThumbPrint, jsonPayload, true);
        }

        public CommonApiResponse<dynamic> DownloadAscAssetData(long supportAccountId, long userAccountId)
        {
            var method = HttpMethod.Get.Method;
            var baseUrl = string.Format(ApiConstants.AscAssetDataDownloadEndpoint, _applicationSettings.ReportingBffHost, supportAccountId, userAccountId);
            return CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.ReportingBffCertThumbPrint, string.Empty, true);
        }

        public CommonApiResponse<dynamic> RegisterTenant(BaseRequest request)
        {
            dynamic result = null;
            try
            {
                var method = HttpMethod.Post.Method;
                var jsonPayload = JsonConvert.SerializeObject(request);
                var baseUrl = string.Format(ApiConstants.RegisterTenant, _applicationSettings.PortalBffHost);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.PortalBffCertThumbPrint, jsonPayload, true);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }

        public RegistrationResult RegisterTenantWithValidation(BaseRequest request)
        {
            var response = RegisterTenant(request);
            if (response?.ResponseObject != null)
            {
                var responseData = response.ResponseObject;

                // ResponseObject is deserialized from the "data" key via [JsonProperty("data")],
                // so responseData already represents: { "status": 4xx, "error_message": [...], "timestamp": "..." }
                bool isDataError = responseData.status != null && (int)responseData.status >= 400;
                if (isDataError)
                {
                    string errorMessage = "Registration request failed. Please try again or contact support.";
                    if (responseData.error_message != null)
                    {
                        var errorMessages = responseData.error_message as Newtonsoft.Json.Linq.JArray;
                        if (errorMessages != null && errorMessages.Count > 0)
                        {
                            var firstEntry = errorMessages[0];
                            var msgToken = firstEntry["error_message"];
                            if (msgToken != null && !string.IsNullOrEmpty(msgToken.ToString()))
                                errorMessage = msgToken.ToString();
                        }
                    }
                    return RegistrationResult.Error(errorMessage);
                }

                return RegistrationResult.Success();
            }
            return RegistrationResult.Error("Registration request failed. Please try again or contact support.");
        }

        public CommonApiResponse<dynamic> UpdateDeviceName(AssetNameUpdateRequest request)
        {
            dynamic result = null;
            try
            {
                var method = HttpMethod.Put.Method;
                var baseUrl = string.Format(ApiConstants.AssetNameUpdateEndpoint, _applicationSettings.ReportingBffHost);
                var jsonPayload = JsonConvert.SerializeObject(request);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.ReportingBffCertThumbPrint, jsonPayload, true);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }

        public CommonApiResponse<dynamic> UpdateLocation(LocationUpdateRequest request)
        {
            dynamic result = null;
            try
            {
                var method = HttpMethod.Put.Method;
                var baseUrl = string.Format(ApiConstants.LocationUpdateEndpoint, _applicationSettings.ReportingBffHost);
                var jsonPayload = JsonConvert.SerializeObject(request);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.ReportingBffCertThumbPrint, jsonPayload, true);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }

        public CommonApiResponse<dynamic> UpdatePanOsVersion(PanOsVersionUpdateRequest request)
        {
            dynamic result = null;
            try
            {
                var method = HttpMethod.Put.Method;
                var baseUrl = string.Format(ApiConstants.PanOsVersionUpdateEndpoint, _applicationSettings.ReportingBffHost);
                var jsonPayload = JsonConvert.SerializeObject(request);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.ReportingBffCertThumbPrint, jsonPayload, true);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }

        public CommonApiResponse<dynamic> UpdateRemarks(RemarksUpdateRequest request)
        {
            dynamic result = null;
            try
            {
                var method = HttpMethod.Put.Method;
                var baseUrl = string.Format(ApiConstants.RemarksUpdateEndpoint, _applicationSettings.ReportingBffHost);
                var jsonPayload = JsonConvert.SerializeObject(request);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.ReportingBffCertThumbPrint, jsonPayload, true);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }

        public CommonApiResponse<dynamic> UpdateAssetTags(AssetTagsUpdateRequest request)
        {
            dynamic result = null;
            try
            {
                var method = HttpMethod.Put.Method;
                var baseUrl = string.Format(ApiConstants.AssetTagsUpdateEndpoint, _applicationSettings.ReportingBffHost);
                var jsonPayload = JsonConvert.SerializeObject(request);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.ReportingBffCertThumbPrint, jsonPayload, true);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }

        public CommonApiResponse<dynamic> ChangetoSpare(SKUChangetoSpareRequest request)
        {
            dynamic result = null;
            try
            {
                var patch = new HttpMethod("PATCH");
                var baseUrl = string.Format(ApiConstants.ChangetoSpareEndpoint, _applicationSettings.ReportingBffHost);
                var jsonPayload = JsonConvert.SerializeObject(request);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, patch.Method, _applicationSettings.ReportingBffCertThumbPrint, jsonPayload, true);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }        

        public CommonApiResponse<dynamic> GetAssetMetaData(long userAccountId, long supportAccountId, string serialNumber)
        {
            dynamic result = null;
            try
            {
                var method = HttpMethod.Get.Method;              
                var baseUrl = string.Format(ApiConstants.OneAssetMetaDataEndpoint, _applicationSettings.ReportingBffHost, supportAccountId, HttpUtility.UrlEncode(serialNumber), userAccountId);                
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.ReportingBffCertThumbPrint, string.Empty, true);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }

        public CommonApiResponse<dynamic> AssetDataInitialLoad(long supportAccountId)
        {
            dynamic result = null;
            try
            {
                var method = HttpMethod.Get.Method;
                var baseUrl = string.Format(ApiConstants.AssetDataInitialLoadEndpoint, _applicationSettings.ReportingBffHost, supportAccountId);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.ReportingBffCertThumbPrint, string.Empty, true);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }
    }
}
