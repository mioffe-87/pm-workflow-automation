using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.HelperClasses;
using Csp.Api.Model;
using Csp.Api.Services.Contracts;
using Csp.Api.Model.Enums;
using Newtonsoft.Json;
using System.Net;
using System.IO;
using System.Web;

namespace Csp.Api.Logic
{
    public class NetworkSecurityApiLogic : INetworkSecurityApiLogic
    {
        private readonly IApplicationSettings _applicationSettings;
        private readonly IDeviceLogic _deviceLogic;
        private readonly IUserAccountLogic _userAccountLogic;

        public NetworkSecurityApiLogic(IApplicationSettings applicationSettings, IDeviceLogic deviceLogic, IUserAccountLogic userAccountLogic)
        {
            _applicationSettings = applicationSettings;
            _deviceLogic = deviceLogic;
            _userAccountLogic = userAccountLogic;
        }

        public CommonApiResponse<dynamic> GetFilters(string filter, long userAccountId, long supportAccountId, int? categoryId)
        {
            dynamic result = null;
            string baseUrl = string.Empty;
            try
            {
                var method = HttpMethod.Get.Method;
                if (categoryId.HasValue)
                {
                    baseUrl = string.Format(ApiConstants.FiltersEndpoint, _applicationSettings.ReportingBffHost, filter, userAccountId, supportAccountId, categoryId);
                }
                else
                {
                    baseUrl = string.Format(ApiConstants.FiltersWithoutCategoryEndpoint, _applicationSettings.ReportingBffHost, filter, userAccountId, supportAccountId);
                }
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.ReportingBffCertThumbPrint, string.Empty, true);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }

        public CommonApiResponse<dynamic> GetAssetData(AssetRequest asssetRequest)
        {
            dynamic result = null;
            try
            {
                var method = HttpMethod.Post.Method;
                var baseUrl = string.Format(ApiConstants.AssetDataEndpoint, _applicationSettings.ReportingBffHost);
                var jsonPayload = JsonConvert.SerializeObject(asssetRequest);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.ReportingBffCertThumbPrint, jsonPayload, true);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }

        public CommonApiResponse<dynamic> GetAssetMetaData(AssetRequest asssetRequest)
        {
            dynamic result = null;
            try
            {
                var method = HttpMethod.Post.Method;
                var baseUrl = string.Format(ApiConstants.AssetMetaDataEndpoint, _applicationSettings.ReportingBffHost);
                var jsonPayload = JsonConvert.SerializeObject(asssetRequest);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.ReportingBffCertThumbPrint, jsonPayload, true);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }

        public CommonApiResponse<dynamic> GetAssetDashboardData(AssetRequest asssetRequest)
        {
            dynamic result = null;
            try
            {
                var method = HttpMethod.Post.Method;
                var baseUrl = string.Format(ApiConstants.AssetMetaDataEndpoint, _applicationSettings.ReportingBffHost);
                var jsonPayload = JsonConvert.SerializeObject(asssetRequest);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.ReportingBffCertThumbPrint, jsonPayload, true);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }

        public CommonApiResponse<dynamic> GetAssetDetails(AssetDetailsRequest assetDetailsRequest)
        {
            dynamic result = null;
            try
            {
                var method = HttpMethod.Post.Method;
                var baseUrl = string.Format(ApiConstants.AssetDetailsEndpoint, _applicationSettings.ReportingBffHost);
                var jsonPayload = JsonConvert.SerializeObject(assetDetailsRequest);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.ReportingBffCertThumbPrint, jsonPayload, true);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }

        public CommonApiResponse<dynamic> GetAuthCodeDetails(long supportAccountId, long userAccountId, string authCode)
        {
            dynamic result = null;
            try
            {
                var method = HttpMethod.Get.Method;
                var baseUrl = string.Format(ApiConstants.AuthCodeDetailsEndpoint, _applicationSettings.ReportingBffHost, supportAccountId, userAccountId, authCode);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.ReportingBffCertThumbPrint, string.Empty, true);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }

        public CommonApiResponse<dynamic> GetDeviceTag(long supportAccountId, long userAccountId, string serialNumber)
        {
            dynamic result = null;
            try
            {
                var method = HttpMethod.Get.Method;
                var baseUrl = string.Format(ApiConstants.AssetTagEndpoint, _applicationSettings.ReportingBffHost, supportAccountId, userAccountId, serialNumber);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.ReportingBffCertThumbPrint, string.Empty, true);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }

        public CommonApiResponse<dynamic> GetDeviceTag(int? page, int? size, string sort, DeviceTagRequest deviceTagRequest)
        {
            dynamic result = null;
            string baseUrl = string.Empty;
            try
            {
                var method = HttpMethod.Post.Method;
                var jsonPayload = JsonConvert.SerializeObject(deviceTagRequest);

                baseUrl = GetBaseUrl(page, size, sort);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.ReportingBffCertThumbPrint, jsonPayload, true);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }

        public CommonApiResponse<dynamic> GetFlexCreditDetails(FlexDetailsRequest flexDetailsRequest)
        {
            dynamic result = null;
            try
            {
                var method = HttpMethod.Post.Method;
                var baseUrl = string.Format(ApiConstants.FlexCreditDetailsEndpoint, _applicationSettings.EntitlementServiceBffHost);
                var jsonPayload = JsonConvert.SerializeObject(flexDetailsRequest);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.EntitlementBffThumbprint, jsonPayload, true);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }
        public CommonApiResponse<dynamic> GetTrialLicenses(long supportAccountId, long userAccountId, string serialNumber)
        {
            dynamic result = null;
            try
            {
                var method = HttpMethod.Get.Method;
                var baseUrl = string.Format(ApiConstants.TrialLicenseEndpoint, _applicationSettings.ReportingBffHost, supportAccountId, userAccountId, serialNumber);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.ReportingBffCertThumbPrint, string.Empty, true);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }

        public CommonApiResponse<TrialLicenseResponseObject> ActivateTrialLicenses(TrialLicenseRequest trialLicenseRequest)
        {
            var method = HttpMethod.Post.Method;
            var jsonPayload = JsonConvert.SerializeObject(trialLicenseRequest);
            var baseUrl = string.Format(ApiConstants.ActivateTrialLicenseEndpoint, _applicationSettings.ReportingBffHost);
            return CommonHelper.ProcessWebRequest<TrialLicenseResponseObject>(baseUrl, method, _applicationSettings.ReportingBffCertThumbPrint, jsonPayload, true);
        }

        public CommonApiResponse<dynamic> ActivateLicense(LicenseRequest licenseRequest)
        {
            dynamic result = null;
            try
            {
                var method = HttpMethod.Post.Method;
                var jsonPayload = JsonConvert.SerializeObject(licenseRequest);
                var baseUrl = string.Format(ApiConstants.ActivateLicenseEndpoint, _applicationSettings.ReportingBffHost);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.ReportingBffCertThumbPrint, jsonPayload, true);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }

        public CommonApiResponse<dynamic> GetPanOSVersions()
        {
            dynamic result = null;
            try
            {
                var method = HttpMethod.Get.Method;
                var baseUrl = string.Format(ApiConstants.PanOSVersionEndpoint, _applicationSettings.PortalBffHost);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.PortalBffCertThumbPrint, string.Empty);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }

        public CommonApiResponse<dynamic> UpdateDeviceInformation(DeviceInformation deviceInfo)
        {
            dynamic result = null;
            try
            {
                var method = HttpMethod.Put.Method;
                var jsonPayload = JsonConvert.SerializeObject(deviceInfo);
                var baseUrl = string.Format(ApiConstants.DeviceInfoEndpoint, _applicationSettings.ReportingBffHost);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.ReportingBffCertThumbPrint, jsonPayload, true);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }

        public CommonApiResponse<dynamic> DownloadAssetData(long supportAccountId, long userAccountId)
        {
            var method = HttpMethod.Get.Method;
            var baseUrl = string.Format(ApiConstants.AssetDataDownloadEndpoint, _applicationSettings.ReportingBffHost, userAccountId, supportAccountId);
            return CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.ReportingBffCertThumbPrint, string.Empty, true);
        }

        public CommonApiResponse<dynamic> GetLicenseActivationTypes(long userAccountId, long supportAccountId, string serialNumber)
        {
            dynamic result = null;
            try
            {
                var method = HttpMethod.Get.Method;
                var baseUrl = string.Format(ApiConstants.LicenseActivationTypesEndpoint, _applicationSettings.ReportingBffHost, userAccountId, supportAccountId, serialNumber);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.ReportingBffCertThumbPrint, string.Empty, true);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }

        public CommonApiResponse<dynamic> GetVirtualPlatforms()
        {
            dynamic result = null;
            try
            {
                var method = HttpMethod.Get.Method;
                var baseUrl = string.Format(ApiConstants.VirtualPlatformsEndpoint, _applicationSettings.ReportingBffHost);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.ReportingBffCertThumbPrint, string.Empty);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }

        private string GetBaseUrl(int? page, int? size, string sort)
        {
            var baseUrl = string.Empty;
            if (page.HasValue && size.HasValue && !string.IsNullOrEmpty(sort))
            {
                baseUrl = string.Format(ApiConstants.AssetTagByIdEndpoint, _applicationSettings.ReportingBffHost, page, size, sort);
            }
            else if (!page.HasValue && !size.HasValue && !string.IsNullOrEmpty(sort))
            {
                baseUrl = string.Format(ApiConstants.AssetTagByIdSortEndpoint, _applicationSettings.ReportingBffHost, sort);
            }
            else if (page.HasValue && size.HasValue && string.IsNullOrEmpty(sort))
            {
                baseUrl = string.Format(ApiConstants.AssetTagByIdWithoutSortEndpoint, _applicationSettings.ReportingBffHost, page, size);
            }
            else
            {
                baseUrl = string.Format(ApiConstants.AssetTagByIdWithoutParamEndpoint, _applicationSettings.ReportingBffHost);
            }

            return baseUrl;
        }

        private void MagicLink(IEnumerable<TrialLicense> iotSaasLicenseList, string userEmail, long supportAccountId, string serialNumber, TrialLicenseResponseObject result)
        {
            string firstIOTLicenseType = string.Empty;
            string combinedMessageText = string.Empty;

            int iotCount = iotSaasLicenseList.Where(p => p.LicenseType.ToUpper().Contains("IOT")).Count();
            var user = _userAccountLogic.GetUserAccountByEmail(userEmail);

            foreach (var license in iotSaasLicenseList)
            {
                string licenseType = string.Empty;
                string messageText = string.Empty;

                try
                {
                    // CSP-3027 CSP Initiated 30 day IOT trial radial button
                    if (license.LicenseType == LicenseFeatureName.IOTSecurity.ToDescription())
                    {
                        licenseType = "IOTSecurity";
                        messageText = "IoT security";
                    }
                    else if (license.LicenseType == LicenseFeatureName.IOTSecurityDataLake.ToDescription())
                    {
                        licenseType = "IOTSecurityDataLake";
                        messageText = "IoT security - Requires Cortex Data Lake";
                    }
                    else if (license.LicenseType == LicenseFeatureName.IOTSecurity3rd.ToDescription())
                    {
                        licenseType = "IOTSecurity3rd";
                        messageText = "IoT security - 3rd Party Integrations add-on";
                    }
                    else if (license.LicenseType == LicenseFeatureName.SaaSInline.ToDescription())
                    {
                        licenseType = "SAASINLINE";
                        messageText = "SaaS Security Inline";
                    }
                    else
                        continue;

                    //If the user selected 2 IOT packages(checkboxes) send one email and one magic link
                    if (iotCount > 1 && license.LicenseType.ToUpper().Contains("IOT") && firstIOTLicenseType == "")
                    {
                        //store the 1st package licenseType, skip to the next item in the loop
                        firstIOTLicenseType = licenseType;
                        combinedMessageText = messageText;
                        continue;
                    }
                    else if (iotCount > 1 && license.LicenseType.ToUpper().Contains("IOT") && firstIOTLicenseType != null)
                    {
                        //concat the 1st & 2nd IOT license type and messageTexts
                        licenseType = firstIOTLicenseType + "," + licenseType;
                        combinedMessageText += " and " + messageText;
                        messageText = combinedMessageText;
                    }

                    string pendingMessage = "Your request to activate the " + messageText +
                                            " trial has been received. Please check your email to activate.";
                    string errorEmailMessage = "Failed to email the " + messageText +
                                               " Activation link. Please contact customer support.";
                    string errorGenerateMessage = "Failed to generate " + messageText +
                                                  " Activation link. Please contact customer support.";

                    IEnumerable<ActivationLinkModel> activeLink =
                        _deviceLogic.GetTrialLicenseActivationActiveLinksBySupportAccountId(
                            supportAccountId, licenseType, serialNumber);

                    if (activeLink != null)
                    {
                        if (user != null && EmailHelper.SendTrialActivationEmail(user.Email,
                                               user.FirstName + " " + user.LastName, messageText,
                                               activeLink))
                        {
                            AddTrialLicenseMessage(result, pendingMessage, string.Empty);
                        }
                        else
                        {
                            AddTrialLicenseMessage(result, string.Empty, errorEmailMessage);
                        }
                    }
                    else
                    {
                        AddTrialLicenseMessage(result, string.Empty, errorGenerateMessage);
                    }
                }
                catch (Exception ex)
                {
                    Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                    AddTrialLicenseMessage(result, string.Empty, "Failed to process Activation Link. Please contact customer support.");
                }
            }
        }

        private void AddTrialLicenseMessage(TrialLicenseResponseObject result, string pendingMessage, string errorMessage)
        {
            if (result != null)
            {
                if(result.Messages == null)
                {
                    result.Messages = new TrialLicenseMessage();
                }
                if (!string.IsNullOrEmpty(pendingMessage))
                {
                    if (result.Messages.Pending == null)
                    {
                        result.Messages.Pending = new List<string>();
                    }
                    result.Messages.Pending.Add(pendingMessage);
                }
                else
                {
                    if (result.Messages.Error == null)
                    {
                        result.Messages.Error = new List<string>();
                    }
                    result.Messages.Error.Add(errorMessage);
                }
            }
        }

        public CommonApiResponse<dynamic> ActivateOfflineLicense(OfflineLicenseRequest licenseRequest)
        {
            dynamic result = null;
            try
            {
                var method = HttpMethod.Post.Method;
                var jsonPayload = JsonConvert.SerializeObject(licenseRequest);
                var baseUrl = string.Format(ApiConstants.ActivateOfflineLicenseEndpoint, _applicationSettings.ReportingBffHost);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.ReportingBffCertThumbPrint, jsonPayload, true);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }

        public CommonApiResponse<dynamic> GetFeatureLicenses(long userAccountId, long supportAccountId, string serialNumber)
        {
            dynamic result = null;
            try
            {
                var method = HttpMethod.Get.Method;
                var baseUrl = string.Format(ApiConstants.GetFeatureLicensesEndpoint, _applicationSettings.ReportingBffHost, supportAccountId, userAccountId, serialNumber);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.ReportingBffCertThumbPrint, string.Empty, true);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }

        public CommonApiResponse<dynamic> RollbackVmUpgrade(long userAccountId, long supportAccountId, string serialNumber, string transactionId)
        {
            dynamic result = null;
            try
            {
                var method = HttpMethod.Post.Method;
                var baseUrl = string.Format(ApiConstants.RollbackUpgradeEndpoint, _applicationSettings.ReportingBffHost, supportAccountId, userAccountId, serialNumber, transactionId);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.ReportingBffCertThumbPrint, string.Empty, true);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }

        public CommonApiResponse<dynamic> GetAscContactInformation(long userAccountId, long supportAccountId, long ascSupportAccountId)
        {
            dynamic result = null;
            try
            {
                var method = HttpMethod.Get.Method;
                var baseUrl = string.Format(ApiConstants.AscContactEndpoint, _applicationSettings.PortalBffHost, userAccountId, supportAccountId, ascSupportAccountId);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.PortalBffCertThumbPrint, string.Empty);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }

        public CommonApiResponse<dynamic> GetDevicesForLicenseTransfer(long userAccountId, long supportAccountId, string serialNumber, bool searchArchive)
        {
            dynamic result = null;
            try
            {
                var method = HttpMethod.Get.Method;
                var baseUrl = string.Format(ApiConstants.DevicesForLicenseTransferEndpoint, _applicationSettings.ReportingBffHost, HttpUtility.UrlEncode(serialNumber), supportAccountId, userAccountId,
                              searchArchive);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.ReportingBffCertThumbPrint, string.Empty);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }

        public CommonApiResponse<dynamic> TransferAssetLicense(TransferAssetLicenseRequest transferAssetLicenseRequest)
        {
            dynamic result = null;
            try
            {
                var method = HttpMethod.Post.Method;
                var jsonPayload = JsonConvert.SerializeObject(transferAssetLicenseRequest);
                var baseUrl = string.Format(ApiConstants.TransferAssetLicenseEndpoint, _applicationSettings.ReportingBffHost);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.ReportingBffCertThumbPrint, jsonPayload);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }

        public CommonApiResponse<dynamic> ActivateSiteLicense(ActivateSiteLicenseRequest activateSiteLicenseRequest)
        {
            dynamic result = null;
            try
            {
                var method = HttpMethod.Post.Method;
                var jsonPayload = JsonConvert.SerializeObject(activateSiteLicenseRequest);
                var baseUrl = string.Format(ApiConstants.ActivateSiteLicenseEndpoint, _applicationSettings.ReportingBffHost);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.ReportingBffCertThumbPrint, jsonPayload);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }

        public CommonApiResponse<dynamic> AssociateComponent(AssociateComponentRequest associateComponentRequest)
        {
            dynamic result = null;
            try
            {
                var method = HttpMethod.Post.Method;
                var jsonPayload = JsonConvert.SerializeObject(associateComponentRequest);
                var baseUrl = string.Format(ApiConstants.AssociateComponentEndpoint, _applicationSettings.ReportingBffHost);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.ReportingBffCertThumbPrint, jsonPayload);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }

        public CommonApiResponse<dynamic> GetUnRegisteredComponents(ComponentRequest componentRequest)
        {
            dynamic result = null;
            try
            {
                var method = HttpMethod.Post.Method;
                var jsonPayload = JsonConvert.SerializeObject(componentRequest);
                var baseUrl = string.Format(ApiConstants.UnRegisteredComponentEndpoint, _applicationSettings.ReportingBffHost);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.ReportingBffCertThumbPrint, jsonPayload);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }

         #region Clai Sec
        public CommonApiResponse<dynamic> GetAssetAttributes(string serialNumber)
        {
            dynamic result = null;
            try
            {
                var method = HttpMethod.Get.Method;
                var baseUrl = string.Format(ApiConstants.AssetAttributesEndpoint, _applicationSettings.EntitlementServiceBffHost, serialNumber);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.EntitlementBffThumbprint, string.Empty);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }
        #endregion


        public CommonApiResponse<dynamic> GenerateMagicLink(MagicLinkRequest magicLinkRequest)
        {
            dynamic result = null;
            try
            {
                var method = HttpMethod.Post.Method;
                var jsonPayload = JsonConvert.SerializeObject(magicLinkRequest);
                var baseUrl = string.Format(ApiConstants.MagicLinkEndpoint, _applicationSettings.ReportingBffHost);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.ReportingBffCertThumbPrint, jsonPayload);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }

        public CommonApiResponse<dynamic> GetAssetLicense(long userAccountId, long supportAccountId, long categoryId, string serialNumber)
        {
            dynamic result = null;
            try
            {
                var method = HttpMethod.Get.Method;
                var baseUrl = string.Format(ApiConstants.AssetLicenseEndpoint, _applicationSettings.ReportingBffHost, categoryId, supportAccountId, userAccountId, serialNumber);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.ReportingBffCertThumbPrint, string.Empty);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;

        }

        /// <summary>
        /// To Delete Expired Trial Licenses
        /// </summary>
        /// <param name="deleteTrialLicenseRequest"></param>
        /// <returns></returns>
        public CommonApiResponse<dynamic> DeleteTrialLicenses(DeleteTrialLicenseRequest deleteTrialLicenseRequest)
        {
            dynamic result = null;

            try
            {
                var method = HttpMethod.Post.Method;
                deleteTrialLicenseRequest.Reason = ApiConstants.DeleteExpiredTrialLicensesReason;
                var jsonPayload = JsonConvert.SerializeObject(deleteTrialLicenseRequest);
                var baseUrl = string.Format(ApiConstants.DeleteTrialLicensesEndpoint, _applicationSettings.ReportingBffHost);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.ReportingBffCertThumbPrint, jsonPayload, true);
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

        public CommonApiResponse<dynamic> ChassisComponents(long userAccountId, long supportAccountId, string serialNumber)
        {
            dynamic result = null;

            try
            {
                var method = HttpMethod.Get.Method;
                var baseUrl = string.Format(ApiConstants.ChassisComponentsEndpoint, _applicationSettings.ReportingBffHost, userAccountId, supportAccountId, serialNumber);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.ReportingBffCertThumbPrint, string.Empty, true);
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
