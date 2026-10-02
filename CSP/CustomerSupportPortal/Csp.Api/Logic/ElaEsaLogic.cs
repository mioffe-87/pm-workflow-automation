using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.Model.Wildfire;
using Csp.Api.Services.Contracts;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Csp.Api.HelperClasses;
using System.Net;
using System.Security.Cryptography;
using System.IO;
using Csp.Api.Model.ElaEsa;
using Csp.Api.Model;

namespace Csp.Api.Logic
{
    public class ElaEsaLogic : BaseLogic, IElaEsaLogic
    {
        private readonly IApplicationSettings applicationSettings = null;
        private readonly IElaEsaDataAccess elaDataAccess = null;
        private readonly IDeviceLogic _deviceLogic;
        public ElaEsaLogic(IElaEsaDataAccess elaDataAccess, IApplicationSettings applicationSettings, IDeviceLogic deviceLogic)
        {
            this.elaDataAccess = elaDataAccess;
            this.applicationSettings = applicationSettings;
            _deviceLogic = deviceLogic;
        }

        public async Task<IEnumerable<ElaEsaCapInfo>> GetElaCapsAsync(long supportAccountId, string authcode = null, int? startMonth = null, int? endMonth = null)
        {
            string url = string.Format("/elaReport?supportAccountId={0}", supportAccountId);
            if (startMonth.HasValue // get historical data
                && endMonth.HasValue)
            {
                DateTime today = DateTime.Now.Date;
                DateTime historyFrom = today.AddMonths(endMonth.Value - startMonth.Value > 0 ? -(endMonth.Value - startMonth.Value) : endMonth.Value - startMonth.Value < 0 ? endMonth.Value - startMonth.Value : 0);
                url += string.Format("&startDateRange={0}-{1}-01&endDateRange={2}-{3}-{4}",
                    historyFrom.Year, historyFrom.Month, today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));
            }
            else
            {
                url += "&usageType=Current";
            }
            var sfdcResult = await QueryBiApiGetAsync<IEnumerable<BiElaEsaCapInfo>>(url);
            IEnumerable<ElaEsaCapInfo> elaEsasInfo = sfdcResult?.ResponseObject?.Where(i => /*i.IsBound &&*/ i.IsValid())
                .Select(i => i.ToElaEsaCapInfo()).OrderBy(i => i.MasterAuthCode).ThenBy(i => i.Date).ToArray();
            if (!string.IsNullOrWhiteSpace(authcode))
            {
                elaEsasInfo = elaEsasInfo.Where(i => i.MasterAuthCode == authcode).ToArray();
            }
            return elaDataAccess.ValidateMasterAuthCodes(supportAccountId, elaEsasInfo);
        }

        public async Task<IEnumerable<ElaEsaDevicePriceInfo>> GetElaDevicePriceAsync(long supportAccountId, string serialNumber)
        {
            // Check if the device is an ELA/ESA device.
            if (!elaDataAccess.ValidateElaDeviceForRegistration(serialNumber))
            {
                return null;
            }

            var sfdcResult = await QueryBiApiGetAsync<IEnumerable<BiElaEsaDevicePriceInfo>>("/elaDetails?supportAccountId=" + supportAccountId + "&serialNumber=" + serialNumber + "&usageType=Current");
            IEnumerable<ElaEsaDevicePriceInfo> devicesPriceInfo = sfdcResult?.ResponseObject?
                .Where(i => i.IsValid()).OrderByDescending(i => i.UsageDate)
                .Where(i => i.SerialNumber == serialNumber).Distinct(new BiElaEsaDevicePriceInfoComparer())
                .Select(i => i.ToElaEsaDevicePriceInfo()).ToArray();
            return devicesPriceInfo;
        }

        public async Task<IEnumerable<ElaEsaDevicePriceInfo>> GetElaDevicesPricesAsync(long supportAccountId, string authcode = null)
        {
            var sfdcResult = await QueryBiApiGetAsync<IEnumerable<BiElaEsaDevicePriceInfo>>("/elaDetails?supportAccountId=" + supportAccountId
                + "&usageType=Current" + (!string.IsNullOrWhiteSpace(authcode) ? "&authcode=" + authcode : string.Empty));

            IEnumerable<ElaEsaDevicePriceInfo> devicesPriceInfo = sfdcResult?.ResponseObject?.Where(i => i.IsValid())
                .OrderByDescending(e => e.UsageDate) //.Distinct(new BiElaEsaDevicePriceInfoComparer())
                                                     //.GroupBy(i => i.SerialNumber, (k, g) => g.OrderByDescending(e => e.UsageDate).First())
                .Select(i => i.ToElaEsaDevicePriceInfo()).ToArray();

            return devicesPriceInfo;
        }

        /// <summary>
        /// Get all ELA/ESA devices eligeable for decommissioning.
        /// </summary>
        /// <param name="supportAccountId"></param>
        /// <returns></returns>
        public IEnumerable<string> GetElaDevicesForDecom(long supportAccountId)
        {
            return elaDataAccess.GetElaDeviceForDecom(supportAccountId);
        }

        public void DecommissionDevices(long supportAccountId, long userAccountId, IEnumerable<ElaEsaDecomissionDevice> decomDevices)
        {
            if (!(decomDevices?.Any() ?? false))
            {
                throw new ApplicationException("Error of decommission ELA/ESA devices. At least one device is required.");
            }

            long sessionId = 0;
            elaDataAccess.PrepDecommissionDevices(supportAccountId, ref decomDevices, out sessionId);
            IEnumerable<ElaEsaDecomissionDevice> validDevices = decomDevices.Where(d => !string.IsNullOrWhiteSpace(d.SerialNumber)).ToArray();
            if (sessionId <= 0)
            {
                throw new ApplicationException("Error of decommission ELA/ESA devices. Failed to generate new Session ID.");
            }

            if (!validDevices.Any())
            {
                elaDataAccess.PostDecommissionDevices(supportAccountId, userAccountId, decomDevices, sessionId, EElaEsaActionStatus.Error,
                    JsonConvert.SerializeObject(new { OriginalPayload = decomDevices, Error = "No API call was made because there is no valid device to decommission." }));
                throw new ApplicationException("Error of decommission ELA/ESA devices. No API call was made because there is no valid device to decommission.");
            }

            ElaEsaDecomissionDevicesSapEcc decomPayloadObject;
            ElaEsaRecycleDevicesSapEcc recyclePayloadObject;
            var isRecycle = validDevices.FirstOrDefault().RecycleDevice;

            recyclePayloadObject = new ElaEsaRecycleDevicesSapEcc(sessionId, validDevices);
            decomPayloadObject = new ElaEsaDecomissionDevicesSapEcc(sessionId, validDevices);

            string payload = isRecycle ?
                 JsonConvert.SerializeObject(recyclePayloadObject) : JsonConvert.SerializeObject(decomPayloadObject);
            try
            {
                // Log the request in Firestore
                WebCommon.BffLogInfo(applicationSettings, payload, "CSP::DecommissionDevices");
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            string baseUrl = applicationSettings.PiApiBffHost + "/CSPDecommission";
            CommonApiResponse<dynamic> result = null;
            try
            {
                using (Task<CommonApiResponse<dynamic>> requestAction = Task.Run(() => CommonHelper.ProcessWebRequestAsync<dynamic>(baseUrl, HttpMethod.Post.Method, applicationSettings.PiApiBffHostThumbprint, payload)))
                {
                    requestAction.Wait();
                    result = requestAction.Result;
                }
                if (isRecycle)
                {
                    elaDataAccess.PostDecommissionDevices(supportAccountId, userAccountId, validDevices, sessionId, EElaEsaActionStatus.InProgress, JsonConvert.SerializeObject(new { Url = baseUrl, OriginalPayload = decomDevices, RequestPayload = recyclePayloadObject }), JsonConvert.SerializeObject(result));
                }
                else
                {
                    elaDataAccess.PostDecommissionDevices(supportAccountId, userAccountId, validDevices, sessionId, EElaEsaActionStatus.InProgress, JsonConvert.SerializeObject(new { Url = baseUrl, OriginalPayload = decomDevices, RequestPayload = decomPayloadObject }), JsonConvert.SerializeObject(result));
                }
            }
            catch (Exception exr)
            {
                if(isRecycle)
                {
                    elaDataAccess.PostDecommissionDevices(supportAccountId, userAccountId, validDevices, sessionId, EElaEsaActionStatus.Error, JsonConvert.SerializeObject(new { Url = baseUrl, OriginalPayload = decomDevices, RequestPayload = recyclePayloadObject, Exception = exr.Message, InnerException = exr.InnerException?.Message }));
                } else
                {
                    elaDataAccess.PostDecommissionDevices(supportAccountId, userAccountId, validDevices, sessionId, EElaEsaActionStatus.Error, JsonConvert.SerializeObject(new { Url = baseUrl, OriginalPayload = decomDevices, RequestPayload = decomPayloadObject, Exception = exr.Message, InnerException = exr.InnerException?.Message }));
                }
                throw;
            }
        }

        public async Task<ElaCapsPagedResponse<IEnumerable<ElaEsaDecomedDevice>>> GetElaDecomHistoryAsync(ElaEsaDecomRequest request)
        {
            return await Task.Run(() => elaDataAccess.GetElaDecomHistory(request));
        }

        public void ElaDecomDevicesComplete(ElaEsaDecomissionDeviceCompleteRequest request)
        {
            // Log the request in Firestore
            string log = request != null ? JsonConvert.SerializeObject(request) : "EMPTY";
            try
            {
                WebCommon.BffLogInfo(applicationSettings, log, "SAP API:ElaDecomDevicesComplete");
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            try
            {
                // Send Request to ReportingBff to initiate Email to Customer
                var successDevices = request.Devices.Where(x => x.Status?.ToLower().Trim() == "success");

                var method = HttpMethod.Post.Method;
                var reportingBffBaseUrl = string.Format(ApiConstants.AssetRecycleEndpoint, applicationSettings.ReportingBffHost);
                var decomDevices = elaDataAccess.GetDecommissionPayloadBySessionId(request.SessionId);
                var successDecomDevices = decomDevices.Where(item1 => successDevices.Any(item2 => item1.SerialNumber == item2.SerialNumber));
                decomDevices.ToList().ForEach(i =>
                {
                    i.Country =!String.IsNullOrEmpty(i.Country)? _deviceLogic.GetCountryNameByCode(i.Country) : null;
                    
                });
                var assetRecycleReq = successDecomDevices?.Select(d => new AssetRecycleRequest(d)).ToArray();
                var jsonPayload = JsonConvert.SerializeObject(new { assetRecycleList = assetRecycleReq });
                CommonHelper.ProcessWebRequest<dynamic>(reportingBffBaseUrl, method, applicationSettings.ReportingBffCertThumbPrint, jsonPayload, true);
            }
            catch(Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            // DB update
            elaDataAccess.ElaDecomDevicesComplete(request, log);

            // dsen TODO: 
            // Push message to Pubsub using NotifyUser end-point
        }

        public Dictionary<long, string> GetElaAssociatedSupportAccounts(long supportAccountId)
        {
            return elaDataAccess.GetElaAssociatedSupportAccounts(supportAccountId);
        }

        public dynamic GetEAStatus(long supportAccountId, string serialNumber)
        {
            dynamic result = null;
            try
            {
                var method = HttpMethod.Get.Method;
                var baseUrl = string.Format(ApiConstants.EAStatusEndpoint, applicationSettings.ReportingBffHost, supportAccountId, serialNumber);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, applicationSettings.ReportingBffCertThumbPrint, string.Empty, true);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }

        private async Task<CommonApiResponse<T>> QueryBiApiGetAsync<T>(string relativeUrl)
        {
            string url = applicationSettings.BiApiBffHost + applicationSettings.BiApiBffConsumptionServices + relativeUrl;
            return await CommonHelper.ProcessWebRequestAsync<T>(url, HttpMethod.Get.Method, applicationSettings.BiApiBffHostThumbprint, null);
        }


        public CommonApiResponse<dynamic> GetEaContracts(long supportAccountId, long userAccountId)
        {
            dynamic result = null;
            try
            {
                var method = HttpMethod.Get.Method;
                string baseUrl = string.Format(ApiConstants.EaContractsEndpoint, applicationSettings.ReportingBffHost, supportAccountId, userAccountId);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, applicationSettings.ReportingBffCertThumbPrint, string.Empty, true);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }

        public CommonApiResponse<dynamic> GetEaContractSummary(string authCode, long supportAccountId, long userAccountId)
        {
            dynamic result = null;
            try
            {
                var method = HttpMethod.Get.Method;
                string baseUrl = string.Format(ApiConstants.EaContractSummaryEndpoint, applicationSettings.ReportingBffHost, supportAccountId, userAccountId, authCode);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, applicationSettings.ReportingBffCertThumbPrint, string.Empty, true);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }

        public CommonApiResponse<dynamic> GetEaAssociatedAccounts(string authCode, long supportAccountId, long userAccountId)
        {
            dynamic result = null;
            try
            {
                var method = HttpMethod.Get.Method;
                string baseUrl = string.Format(ApiConstants.EaAssociatedAccountsEndpoint, applicationSettings.ReportingBffHost, supportAccountId, userAccountId, authCode);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, applicationSettings.ReportingBffCertThumbPrint, string.Empty, true);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }

        public dynamic GetEaTotalFirewalls(string authCode, long supportAccountId, long userAccountId, bool export = false)
        {
            dynamic result = null;
            try
            {
                var method = HttpMethod.Get.Method;
                string baseUrl = string.Format(ApiConstants.EaTotalFirewallsEndpoint, applicationSettings.ReportingBffHost, authCode, supportAccountId, userAccountId, export);
                if (!export)
                    result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, applicationSettings.ReportingBffCertThumbPrint, string.Empty, true);
                else
                    result = CommonHelper.ProcessWebRequest(baseUrl, method, applicationSettings.ReportingBffCertThumbPrint, string.Empty, true);
            }

            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }

        public CommonApiResponse<dynamic> GetEaContractsHistory(long supportAccountId, long userAccountId, string authCode)
        {
            dynamic result = null;
            try
            {
                var method = HttpMethod.Get.Method;
                string baseUrl = string.Format(ApiConstants.EaContractsHistoryEndpoint, applicationSettings.ReportingBffHost, supportAccountId, userAccountId, authCode);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, applicationSettings.ReportingBffCertThumbPrint, string.Empty, true);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }
    }
}
