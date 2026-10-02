using Csp.Api.Model.ElaEsa;
using Csp.Api.Model.Enums;
using Csp.Api.Model.Wildfire;
using Csp.Api.Security;
using Csp.Api.Services.Contracts;
using Csp.Web.Attributes;
using Csp.Web.Extensions;
using Csp.Web.Filters;
using Csp.Web.HelperClasses;
using Csp.Web.Security.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;

namespace Csp.Web.Controllers
{
    [Route("api/[controller]")]
    public class ElaEsaApiController : ApiController
    {
        private readonly IElaEsaLogic elaLogic = null;
        private readonly IDeviceLogic _deviceLogic;
        private readonly ISecurityLogic _securityLogic;

        public ElaEsaApiController(IElaEsaLogic elaLogic, IDeviceLogic deviceLogic, ISecurityLogic securityLogic)
        {
            this.elaLogic = elaLogic;
            _deviceLogic = deviceLogic;
            _securityLogic = securityLogic;
        }

        /// <summary>
        /// Get ELA/ESA Info for specified supportAccountId.
        /// </summary>
        /// <example>
        /// Case 1: Enterprise Agreement page / Consumption tab.
        /// On component load, call this endpoint to get all ELA/ESA Caps for the CSP ID.
        /// </example>
        /// <example>
        /// Case 2: User interaction Device Registration SN textbox focus lost triggers validation 
        /// /api/devicevalidation/ValidateSerialNumber. This end-point needs to be updated to call 
        /// this method to get all ELA/ESA Caps for the CSP ID; the call is parallel to call of GetElaDevicePrice. 
        /// Both calls fetch both parts of the object, which UI needs to assemble.
        /// </example>
        /// <example>
        /// Case 3: Decommission Devices component; on load event, get all ELA/ESA Caps for the CSP ID to provide 
        /// starting data for dynamic caclulation of future ELA/ESA monetary state when a device is selected for decom.
        /// </example>
        /// <param name="supportAccountId">Support Account ID to get collection of the active ELA/ESA contracts.</param>
        /// <param name="userAccountId">User Account to check if user has permissions.</param>
        /// <returns>Status of the request with collection of ELA/ESA info - ElaEsaCapInfo objects.</returns>
        [JwtTokenAuthentication]
        [HttpGet]
        public async Task<IHttpActionResult> GetElaCapsAsync(long supportAccountId, long userAccountId, string authcode = null, int? startMonth = null, int? endMonth = null)
        {
            if (supportAccountId <= 0)
            {
                string message = "Error of getting ELA/ESA Caps. Valid support Account ID is required.";
                Elmah.ErrorSignal.FromCurrentContext().Raise(new ArgumentException(message));
                return WebExtensions.HttpCustomErrorResponse(HttpStatusCode.NotFound, this, new string[] { message });
            }

            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId))
            {
                string message = string.Format("User {0} does not have permissions to get ELA/ESA Caps.", userAccountId);
                Elmah.ErrorSignal.FromCurrentContext().Raise(new UnauthorizedAccessException(message));
                return WebExtensions.HttpCustomErrorResponse(HttpStatusCode.Unauthorized, this, new string[] { message });
            }

            try
            {

                ElaCapsResponse<IEnumerable<ElaEsaCapInfo>> response = new ElaCapsResponse<IEnumerable<ElaEsaCapInfo>> { Status = ElaCapsResponse<IEnumerable<ElaEsaCapInfo>>.MESSAGE_SUCCESS };

                //string elaImpactedCspIds = ConfigurationManager.AppSettings["ELAImpacted"];
                //string[] exclusionList = elaImpactedCspIds.Split(',');
                //bool isExcluded = exclusionList.Any(x => x.Equals(supportAccountId.ToString()));
                //if (isExcluded)
                //{
                //    return Ok(response);
                //}

                response.Data = await elaLogic.GetElaCapsAsync(supportAccountId, authcode, startMonth, endMonth);
                return Ok(response);
            }
            catch (Exception ex)
            {
                string message = "Error of getting ELA Caps.";
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return WebExtensions.HttpCustomErrorResponse(HttpStatusCode.ServiceUnavailable, this, new string[] { message });
            }
        }

        /// <summary>
        /// Check if the device is ELA/ESA device and if it is, get the price info of the device.
        /// <example>
        /// Case: User interaction Device Registration SN textbox focus lost triggers validation 
        /// /api/devicevalidation/ValidateSerialNumber. This end-point needs to be updated to call 
        /// this method to check if the SN is an ELA/ESA device.
        /// If it is, the response object needs to be extended to add ElaCapsResponse.
        /// </example>
        /// </summary>
        /// <param name="supportAccountId">Support Account ID to get collection of the active ELA/ESA device price.</param>
        /// <param name="userAccountId">User Account to check if user has permissions to get active ELA/ESA device(s) price(s).</param>
        /// <param name="serialNumber">SN of the device to check for ELA/ESA SKU and get price info of.</param>
        /// <returns>ElaCapsResponse result object, which includes Device Price Info.</returns>
        [JwtTokenAuthentication]
        [HttpGet]
        public async Task<IHttpActionResult> GetElaDevicePriceAsync(long supportAccountId, long userAccountId, string serialNumber)
        {
            if (supportAccountId <= 0)
            {
                string message = "Error of getting ELA/ESA device price. Valid support Account ID is required.";
                Elmah.ErrorSignal.FromCurrentContext().Raise(new ArgumentException(message));
                return WebExtensions.HttpCustomErrorResponse(HttpStatusCode.NotFound, this, new string[] { message });
            }

            if (string.IsNullOrWhiteSpace(serialNumber))
            {
                string message = "Error of getting ELA/ESA device price. Valid Serial Number is required.";
                Elmah.ErrorSignal.FromCurrentContext().Raise(new UnauthorizedAccessException(message));
                return WebExtensions.HttpCustomErrorResponse(HttpStatusCode.NotFound, this, new string[] { message });
            }

            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId))
            {
                string message = string.Format("User {0} does not have permissions to get ELA/ESA Device Price.", userAccountId);
                Elmah.ErrorSignal.FromCurrentContext().Raise(new UnauthorizedAccessException(message));
                return WebExtensions.HttpCustomErrorResponse(HttpStatusCode.Unauthorized, this, new string[] { message });
            }

            try
            {
                ElaCapsResponse<IEnumerable<ElaEsaDevicePriceInfo>> response = new ElaCapsResponse<IEnumerable<ElaEsaDevicePriceInfo>> { Status = ElaCapsResponse<ElaEsaDevicePriceInfo>.MESSAGE_SUCCESS };
                response.Data = await elaLogic.GetElaDevicePriceAsync(supportAccountId, serialNumber);
                return Ok(response);
            }
            catch (Exception ex)
            {
                string message = "Error of getting ELA device price.";
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return WebExtensions.HttpCustomErrorResponse(HttpStatusCode.ServiceUnavailable, this, new string[] { message });
            }
        }

        /// <summary>
        /// Get prices info for all ELA/ESA devices for a specified Support Account ID.
        /// <example>
        /// Case: Decommission Devices component; on load event, get prices info for all ELA/ESA devices of the specified 
        /// Support Account ID to participate in caclulation of future ELA/ESA monetary state when a device is 
        /// selected for decom.
        /// </example>
        /// </summary>
        /// <param name="supportAccountId">Support Account ID to get collection of the active ELA/ESA device(s) price(s).</param>
        /// <param name="userAccountId">User Account to check if user has permissions to get active ELA/ESA device(s) price(s).</param>
        /// <returns>ElaCapsResponse result object, which includes Devices Prices Info collection.</returns>
        [JwtTokenAuthentication]
        [HttpGet]
        public async Task<IHttpActionResult> GetElaDevicesPricesAsync(long supportAccountId, long userAccountId, bool onlyForDecomissioning = false, string authcode = null)
        {
            if (supportAccountId <= 0)
            {
                string message = "Error of getting ELA/ESA devices prices. Valid support Account ID is required.";
                Elmah.ErrorSignal.FromCurrentContext().Raise(new ArgumentException(message));
                return WebExtensions.HttpCustomErrorResponse(HttpStatusCode.NotFound, this, new string[] { message });
            }

            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId))
            {
                string message = string.Format("User {0} does not have permissions to get ELA/ESA Devices Prices.", userAccountId);
                Elmah.ErrorSignal.FromCurrentContext().Raise(new UnauthorizedAccessException(message));
                return WebExtensions.HttpCustomErrorResponse(HttpStatusCode.Unauthorized, this, new string[] { message });
            }

            try
            {
                ElaCapsResponse<IEnumerable<ElaEsaDevicePriceInfo>> response = new ElaCapsResponse<IEnumerable<ElaEsaDevicePriceInfo>> { Status = ElaCapsResponse<IEnumerable<ElaEsaDevicePriceInfo>>.MESSAGE_SUCCESS };
                IEnumerable<ElaEsaDevicePriceInfo> prices = await elaLogic.GetElaDevicesPricesAsync(supportAccountId, authcode);
                if (onlyForDecomissioning)
                {
                    IEnumerable<string> cspSns = elaLogic.GetElaDevicesForDecom(supportAccountId);
                    response.Data = prices.Where(i => cspSns.Any(s => s == i.SerialNumber)).ToArray();
                }
                else
                {
                    response.Data = prices;
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                string message = "Error of getting ELA Devices Prices.";
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return WebExtensions.HttpCustomErrorResponse(HttpStatusCode.ServiceUnavailable, this, new string[] { message });
            }
        }

        [JwtTokenAuthentication]
        [HttpPost]
        public async Task<IHttpActionResult> GetElaSelectedDevicesPricesAsync([FromBody] string[] sns, long supportAccountId, long userAccountId)
        {
            if (supportAccountId <= 0)
            {
                string message = "Error of getting ELA/ESA selected devices prices. Valid support Account ID is required.";
                return WebExtensions.HttpCustomErrorResponse(HttpStatusCode.NotFound, this, new string[] { message });
            }

            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId))
            {
                string message = string.Format("User {0} does not have permissions to get ELA/ESA selected devices prices.", userAccountId);
                return WebExtensions.HttpCustomErrorResponse(HttpStatusCode.Unauthorized, this, new string[] { message });
            }

            try
            {
                ElaCapsResponse<IEnumerable<ElaEsaDevicePriceInfo>> response = new ElaCapsResponse<IEnumerable<ElaEsaDevicePriceInfo>> { Status = ElaCapsResponse<IEnumerable<ElaEsaDevicePriceInfo>>.MESSAGE_SUCCESS };
                if (sns?.Any() ?? false)
                {
                    IEnumerable<ElaEsaDevicePriceInfo> prices = await elaLogic.GetElaDevicesPricesAsync(supportAccountId);
                    prices = prices.Where(i => sns.Any(s => i.SerialNumber == s)).ToArray();
                    IEnumerable<string> cspSns = elaLogic.GetElaDevicesForDecom(supportAccountId);
                    response.Data = prices.Where(i => cspSns.Any(s => s == i.SerialNumber)).ToArray();
                }
                else
                {
                    response.Data = new ElaEsaDevicePriceInfo[0];
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                string message = "Error of getting ELA Selected Devices Prices.";
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return WebExtensions.HttpCustomErrorResponse(HttpStatusCode.ServiceUnavailable, this, new string[] { message });
            }
        }

        [JwtTokenAuthentication]
        [HttpPut]
        public IHttpActionResult DecommissionDevices([FromBody] ElaEsaDecomissionDevice[] decomDevices, long supportAccountId, long userAccountId)
        {
            if (!(decomDevices?.Any() ?? false))
            {
                string message = "Error of decommission ELA/ESA devices. Details are required.";
                Elmah.ErrorSignal.FromCurrentContext().Raise(new ArgumentException(message));
                return WebExtensions.HttpCustomErrorResponse(HttpStatusCode.NotFound, this, new string[] { message });
            }

            if (supportAccountId <= 0)
            {
                string message = "Error of decommission ELA/ESA devices. Valid support Account ID is required.";
                Elmah.ErrorSignal.FromCurrentContext().Raise(new UnauthorizedAccessException(message));
                return WebExtensions.HttpCustomErrorResponse(HttpStatusCode.NotFound, this, new string[] { message });
            }

            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId))
            {
                string message = string.Format("User {0} does not have permissions to decom ELA/ESA devices.", userAccountId);
                Elmah.ErrorSignal.FromCurrentContext().Raise(new UnauthorizedAccessException(message));
                return WebExtensions.HttpCustomErrorResponse(HttpStatusCode.Unauthorized, this, new string[] { message });
            }

            List<string> errors = new List<string>();
            decomDevices.ToList().ForEach(i =>
            {
                i.Country = i.CountryId > 0 ? _deviceLogic.GetCountryCodeById(i.CountryId) : null;
                i.SupportAccountId = supportAccountId;
                i.UserAccountId = userAccountId;
                var deviceErrors = i.Validate();
                if (deviceErrors != null)
                {
                    errors.AddRange(deviceErrors);
                }
            });
            if (errors.Any())
            {
                string message = "Error of decommission ELA/ESA devices. " + string.Join("\r\n", errors);
                return WebExtensions.HttpCustomErrorResponse(HttpStatusCode.ServiceUnavailable, this, new string[] { message });
            }

            try
            {
                elaLogic.DecommissionDevices(supportAccountId, userAccountId, decomDevices);
                return Ok();
            }
            catch (Exception ex)
            {
                string message = "Error of decommission ELA/ESA devices.";
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return WebExtensions.HttpCustomErrorResponse(HttpStatusCode.ServiceUnavailable, this, new string[] { message });
            }
        }

        /// <summary>
        /// Get ELA/ESA decommission devices history for a specified Support Account ID.
        /// </summary>
        /// <param name="supportAccountId">Support Account ID.</param>
        /// <param name="userAccountId">User Account ID.</param>
        /// <param name="pageNumber">Page number of pagination.</param>
        /// <param name="perPage">Number of items per page of pagination.</param>
        /// <param name="sortColumn">Name of sorting column, if any. Opional.</param>
        /// <param name="sortDirection">Sorting direction, if any. Opional. Values ASC, DESC or NULL</param>
        /// <returns>ElaCapsResponse result object, which includes decommissioned devices history objects collection.</returns>
        [JwtTokenAuthentication]
        [HttpPost]
        public async Task<IHttpActionResult> GetElaDecomHistoryAsync([FromBody] ElaEsaDecomRequest request)
        {
            if (request == null)
            {
                string message = "Error of getting ELA/ESA decommission devices history. Request parameters are required.";
                return WebExtensions.HttpCustomErrorResponse(HttpStatusCode.NotAcceptable, this, new string[] { message });
            }

            if (request.Params == null)
            {
                string message = "Error of getting ELA/ESA decommission devices history. Request Support and User accounts are required.";
                return WebExtensions.HttpCustomErrorResponse(HttpStatusCode.NotAcceptable, this, new string[] { message });
            }

            if (request.Data == null)
            {
                string message = "Error of getting ELA/ESA decommission devices history. Request pagination parameters are required.";
                return WebExtensions.HttpCustomErrorResponse(HttpStatusCode.NotAcceptable, this, new string[] { message });
            }

            if (request.Params.SupportAccountId <= 0)
            {
                string message = "Error of getting ELA/ESA decommission devices history. Valid support Account ID is required.";
                return WebExtensions.HttpCustomErrorResponse(HttpStatusCode.NotFound, this, new string[] { message });
            }

            if (!JwtTokenManager.IsAuthorized(request.Params.UserAccountId, request.Params.SupportAccountId, ClaimType.UserAccountId)|| !_securityLogic.HasPermission(request.Params.SupportAccountId, request.Params.UserAccountId, Permission.EditDevice))
            {
                string message = string.Format("User {0} does not have permissions to get ELA/ESA Decom History.", request.Params.UserAccountId);
                return WebExtensions.HttpCustomErrorResponse(HttpStatusCode.Unauthorized, this, new string[] { message });
            }

            try
            {
                return Ok(await elaLogic.GetElaDecomHistoryAsync(request));
            }
            catch (Exception ex)
            {
                string message = "Error of getting ELA/ESA decommission devices history.";
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return WebExtensions.HttpCustomErrorResponse(HttpStatusCode.ServiceUnavailable, this, new string[] { message });
            }
        }

        [JwtTokenAuthentication]
        [HttpGet]
        public async Task<ResponseMessageResult> GetElaDecomHistoryDownloadAsync(long supportAccountId, long userAccountId, string sortColumn = null, string sortDirection = null)
        {
            if (supportAccountId <= 0)
            {
                string message = "Error of download ELA/ESA decommission devices history. Valid support Account ID is required.";
                ArgumentException ex = new ArgumentException(message);
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                var errorResponse = new HttpResponseMessage(HttpStatusCode.NotFound)
                    {
                        Content = new StringContent(JsonConvert.SerializeObject(new { 
                            data = new { 
                                status = 404, 
                                error_message = new[] { new { error_message = message } },
                                timestamp = DateTime.Now.ToString("o")
                            }
                        }))
                    };
                    return ResponseMessage(errorResponse);
            }

            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId) || !_securityLogic.HasPermission(supportAccountId, userAccountId, Permission.EditDevice))
            {
                string message = string.Format("User {0} does not have permissions to download ELA/ESA Decom History.", userAccountId);
                ArgumentException ex = new ArgumentException(message);
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                var errorResponse = new HttpResponseMessage(HttpStatusCode.NotFound)
                    {
                        Content = new StringContent(JsonConvert.SerializeObject(new { 
                            data = new { 
                                status = 404, 
                                error_message = new[] { new { error_message = message } },
                                timestamp = DateTime.Now.ToString("o")
                            }
                        }))
                    };
                    return ResponseMessage(errorResponse);
            }
            StringBuilder sb = new StringBuilder();
            try
            {
                IEnumerable<ElaEsaDecomedDevice> data = (await elaLogic.GetElaDecomHistoryAsync(new ElaEsaDecomRequest
                {
                    Params = new ElaEsaDecomRequestParams
                    {
                        SupportAccountId = supportAccountId
                    },
                    Data = new ElaEsaDecomRequestData
                    {
                        CurrentPage = 1,
                        PageSize = int.MaxValue,
                        SortField = sortColumn,
                        SortOrder = sortDirection
                    }
                }))?.Data ?? new ElaEsaDecomedDevice[0];
               
                sb.AppendFormat("{0}\r\n", ElaEsaDecomedDevice.CVS_HEADER);
                data?.ToList().ForEach(d => sb.AppendFormat("{0}", d.CsvData));
                HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes(sb.ToString())));
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                response.Content.Headers.Add("Access-Control-Expose-Headers", "Content-Disposition,X-Suggested-Filename");
                response.Content.Headers.ContentDisposition.FileName = string.Format("DecommHistory_{0}_{1}.csv", supportAccountId, DateTime.Now.Date.ToString("MM-dd-yyyy"));
                return ResponseMessage(response);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw new HttpException("Error of download ELA/ESA decommission devices history.");
            }
        }

        [HttpPut]
        //[RequireCspApiCert]
        public IHttpActionResult ElaDecomDevicesComplete([FromBody] ElaEsaDecomissionDeviceCompleteRequest request)
        {
            if (request == null)
            {
                string message = "Error of getting ELA/ESA decommission devices complete. Details are required.";
                return WebExtensions.HttpCustomErrorResponse(HttpStatusCode.NotAcceptable, this, new string[] { message });
            }

            List<string> requestErrors = new List<string>();
            if (string.IsNullOrWhiteSpace(request.SessionId))
            {
                requestErrors.Add("Session ID is required.");
            }

            if (request.Status == "success"
                && (request.Devices == null
                || !request.Devices.Any()))
            {
                requestErrors.Add("At least one device in the 'devices' property is required.");
            }

            if (string.IsNullOrWhiteSpace(request.Status))
            {
                requestErrors.Add("Status 'success' or 'error' is required.");
            }

            if (requestErrors.Any())
            {
                return WebExtensions.HttpCustomErrorResponse(HttpStatusCode.NotAcceptable, this, requestErrors.ToArray());
            }

            try
            {
                elaLogic.ElaDecomDevicesComplete(request);
                return Ok();
            }
            catch (Exception ex)
            {
                string message = "Error of getting ELA/ESA decommission devices complete.";
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return WebExtensions.HttpCustomErrorResponse(HttpStatusCode.ServiceUnavailable, this, new string[] { message });
            }
        }

        [JwtTokenAuthentication]
        [HttpGet]
        public IHttpActionResult GetElaAssociatedSupportAccounts(long supportAccountId, long userAccountId)
        {
            if (supportAccountId <= 0)
            {
                string message = "Error of getting ELA/ESA Associated Support Accounts. Valid support Account ID is required.";
                return WebExtensions.HttpCustomErrorResponse(HttpStatusCode.NotFound, this, new string[] { message });
            }

            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId))
            {
                string message = string.Format("User {0} does not have permissions to get ELA/ESA Associated Support Accounts.", userAccountId);
                return WebExtensions.HttpCustomErrorResponse(HttpStatusCode.Unauthorized, this, new string[] { message });
            }

            try
            {
                return Ok(elaLogic.GetElaAssociatedSupportAccounts(supportAccountId));
            }
            catch (Exception ex)
            {
                string message = "Error of getting ELA Associated Support Accounts.";
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return WebExtensions.HttpCustomErrorResponse(HttpStatusCode.ServiceUnavailable, this, new string[] { message });
            }
        }

        [JwtTokenAuthentication]
        [HttpGet]
        public IHttpActionResult GetEaContracts(long supportAccountId, long userAccountId)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId))
            {
                return Unauthorized();
            }
            try
            {
                var result = elaLogic.GetEaContracts(supportAccountId, userAccountId);
                return this.Json(result);
            }
            catch (Exception ex)
            {
                string message = "Error of getting EA Contracts.";
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return WebExtensions.HttpCustomErrorResponse(HttpStatusCode.ServiceUnavailable, this, new string[] { message });
            }
            
        }

        [JwtTokenAuthentication]
        [HttpGet]
        public async Task<IHttpActionResult> GetEaContractSummary(string authcode, long supportAccountId, long userAccountId)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId))
            {
                return Unauthorized();
            }
            try
            {
                var result = elaLogic.GetEaContractSummary(authcode, supportAccountId, userAccountId);
                return this.Json(result);
            }
            catch (Exception ex)
            {
                string message = "Error of getting EA Contract Summary.";
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return WebExtensions.HttpCustomErrorResponse(HttpStatusCode.ServiceUnavailable, this, new string[] { message });
            }
           
        }

        [JwtTokenAuthentication]
        [HttpGet]
        public async Task<IHttpActionResult> GetEaAssociatedAccounts(string authcode, long supportAccountId, long userAccountId)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId))
            {
                return Unauthorized();
            }
            try
            {
                var result = elaLogic.GetEaAssociatedAccounts(authcode, supportAccountId, userAccountId);
                return this.Json(result);
            }
            catch (Exception ex)
            {
                string message = "Error of getting EA Associated Accounts.";
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return WebExtensions.HttpCustomErrorResponse(HttpStatusCode.ServiceUnavailable, this, new string[] { message });
            }
            
        }

        [JwtTokenAuthentication]
        [HttpGet]
        public async Task<IHttpActionResult> GetEaTotalFirewalls(string authcode, long supportAccountId, long userAccountId, bool export = false)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId))
            {
                return Unauthorized();
            }
            try
            {
                var result = elaLogic.GetEaTotalFirewalls(authcode, supportAccountId, userAccountId, export);
                if (export == true)
                {
                    MemoryStream stream = new MemoryStream();
                    StreamWriter writer = new StreamWriter(stream);
                    writer.Write(result.ToString());
                    writer.Flush();
                    stream.Position = 0;
                    var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                    httpResponseMessage.Content = new StreamContent(stream);
                    httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    httpResponseMessage.Content.Headers.Add("Access-Control-Expose-Headers", "Content-Disposition,X-Suggested-Filename");
                    httpResponseMessage.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                    {
                        FileName = "EA_" + authcode + ".csv"
                    };

                    return ResponseMessage(httpResponseMessage);
                }
                else
                {
                    return this.Json(result);
                }


            }
            catch (Exception ex)
            {
                string message = "Error of getting EA Total Associated Firewalls.";
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return WebExtensions.HttpCustomErrorResponse(HttpStatusCode.ServiceUnavailable, this, new string[] { message });
            }
        }

        [HttpGet]
        [JwtTokenAuthentication]
        public IHttpActionResult GetEAStatus(long supportAccountId, long userAccountId, string serialNumber)
        {

            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId))
            {
                return Unauthorized();
            }
            try
            {
                var result = elaLogic.GetEAStatus(supportAccountId, serialNumber);
                return this.Json(result);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(string.Empty);
        }

        [JwtTokenAuthentication]
        [HttpGet]
        public IHttpActionResult GetEaContractsHistory(long supportAccountId, long userAccountId, string authCode)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId))
            {
                return Unauthorized();
            }
            try
            {
                var result = elaLogic.GetEaContractsHistory(supportAccountId, userAccountId, authCode);
                return this.Json(result);
            }
            catch (Exception ex)
            {
                string message = "Error of getting EA Contracts History.";
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return WebExtensions.HttpCustomErrorResponse(HttpStatusCode.ServiceUnavailable, this, new string[] { message });
            }

        }

    }
}
