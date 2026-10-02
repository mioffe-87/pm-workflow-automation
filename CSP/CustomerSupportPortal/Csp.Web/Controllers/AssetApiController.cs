using AutoMapper;
using Csp.Api.DataAccess;
using Csp.Api.HelperClasses;
using Csp.Api.Model;
using Csp.Api.Model.Enums;
using Csp.Api.Security;
using Csp.Api.Services.Contracts;
using Csp.Web.Filters;
using Csp.Web.HelperClasses;
using Csp.Web.Security.Attributes;
using Microsoft.Security.Application;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using ValidateAntiForgeryToken = System.Web.Mvc.ValidateAntiForgeryTokenAttribute;

namespace Csp.Web.Controllers
{
    public class AssetApiController : ApiController
    {
        private readonly IAssetApiLogic _assetApiLogic;
        private readonly IDeviceTransferLogic _deviceTransferLogic;
        private readonly IWildfireWipeoutService _wildfireWipeoutService;
        private readonly IBootstrapContainerLogic _bootstrapContainerLogic;
        private readonly ISecurityLogic _securityLogic;
        private readonly IUserAccountLogic _userAccountLogic;

        public AssetApiController(IDeviceTransferLogic deviceTransferLogic, IWildfireWipeoutService wildfireWipeoutService, IBootstrapContainerLogic bootstrapContainerLogic,
            ISecurityLogic securityLogic, IUserAccountLogic userAccountLogic,
            IAssetApiLogic assetApiLogic)
        {
            _deviceTransferLogic = deviceTransferLogic;
            _wildfireWipeoutService = wildfireWipeoutService;
            _bootstrapContainerLogic = bootstrapContainerLogic;
            _securityLogic = securityLogic;
            _userAccountLogic = userAccountLogic;
            _assetApiLogic = assetApiLogic;
        }

        [HttpPost, ValidateAntiForgeryToken]
        [JwtTokenAuthentication]
        public async Task<IHttpActionResult> BeginTransfer([FromBody] DeviceTransferPayload deviceTransfer)
        {
            if (!JwtTokenManager.IsAuthorized(deviceTransfer.UserAccountId.ToString(), ClaimType.UserAccountId))
            {
                return Unauthorized();
            }

            try
            {
                // Sanitize input to prevent XSS attacks
                var sanitizedDeviceTransfer = SanitizeDeviceTransferPayload(deviceTransfer);
                var response = _assetApiLogic.BeginTransfer(sanitizedDeviceTransfer);
                return this.Json(response);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(string.Empty);
        }

        private DeviceTransferPayload SanitizeDeviceTransferPayload(DeviceTransferPayload payload)
        {
            if (payload == null) return null;

            const string escapePlus = @"&#43;";
            payload.DestinationUserEmail = !string.IsNullOrEmpty(payload.DestinationUserEmail)
                    ? Sanitizer.GetSafeHtmlFragment(payload.DestinationUserEmail).Replace(escapePlus, "+")
                    : payload.DestinationUserEmail;

            payload.EulaId = !string.IsNullOrEmpty(payload.EulaId)
                  ? Sanitizer.GetSafeHtmlFragment(payload.EulaId)
                  : payload.EulaId;

            payload.SerialNumber = !string.IsNullOrEmpty(payload.SerialNumber)
                   ? Sanitizer.GetSafeHtmlFragment(payload.SerialNumber)
                   : payload.SerialNumber;
            return payload;
        }


        [HttpPost, ValidateAntiForgeryToken]
        [JwtTokenAuthentication]
        public IHttpActionResult InitiateBulkTransfer(long userAccountId, [FromBody] DeviceTransferBulkRequest bulkDeviceTransferRequest)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId.ToString(), ClaimType.UserAccountId))
            {
                return Unauthorized();
            }

            try
            {
                var result = _assetApiLogic.InitiateBulkTransfer(bulkDeviceTransferRequest);
                return this.Json(result);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(string.Empty);
        }


        [HttpPost]
        [JwtTokenAuthentication]
        public IHttpActionResult ReviewTransfer([FromBody] DeviceTransferReviewRequest deviceTransferReview)
        {
            if (!JwtTokenManager.IsAuthorized(deviceTransferReview.UserAccountId.ToString(), ClaimType.UserAccountId))
            {
                return Unauthorized();
            }

            try
            {
                // Sanitize input to prevent XSS attacks
                var sanitizedRequest = SanitizeDeviceTransferReviewRequest(deviceTransferReview);
                var response = _assetApiLogic.ReviewTransfer(sanitizedRequest);
                return this.Json(response);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(string.Empty);
        }

        private DeviceTransferReviewRequest SanitizeDeviceTransferReviewRequest(DeviceTransferReviewRequest request)
        {
            if (request == null) return null;
            const string escapePlus = @"&#43;";
            request.SerialNumber = !string.IsNullOrEmpty(request.SerialNumber)
                    ? Sanitizer.GetSafeHtmlFragment(request.SerialNumber)
                    : request.SerialNumber;
            request.Email = !string.IsNullOrEmpty(request.Email)
                    ? Sanitizer.GetSafeHtmlFragment(request.Email).Replace(escapePlus, "+")
                    : request.Email;
            return request;
        }


        [HttpGet]
        [JwtTokenAuthentication]
        public IHttpActionResult WipeOutNFRWildfire(string serialnumber)
        {
            var status = false;
            try
            {
                status = _wildfireWipeoutService.WildFireWipeout(serialnumber);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return this.Json(new
            {
                data = new
                {
                    success = status,
                    message = status ? string.Format(ApiConstants.WildfireDeleteSuccessMessage, ApiConstants.WILDFIRENFR)
                    : string.Format(ApiConstants.WildfireDeleteFailedMessage, ApiConstants.WILDFIRENFR)
                }
            });
        }

        [HttpPost]
        [JwtTokenAuthentication]
        public IHttpActionResult BootstrapContainerDownload(long userAccountId)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId.ToString(), ClaimType.UserAccountId))
            {
                return Unauthorized();
            }

            int index = 0;
            string message = string.Empty;
            var date = DateTime.Now.ToString("yyyyMMdd");
            var deviceDataAccess = new DeviceDataAccess();

            try
            {
                var httpRequest = HttpContext.Current.Request;
                var deviceId = deviceDataAccess.GetDeviceIdBySerialNumber(httpRequest.Form["serialNumber"]);
                HttpPostedFileBase[] filebase = new HttpPostedFileBase[httpRequest.Files.Count];

                if (httpRequest.Files.Count > 0)
                {
                    foreach (string fName in httpRequest.Files)
                    {
                        HttpPostedFile file = httpRequest.Files[fName];
                        filebase[index] = new HttpPostedFileWrapper(file);
                        index++;
                    }
                }
                var model = _bootstrapContainerLogic.PrepareDownload(deviceId, userAccountId, filebase.ToArray());

                if (model.IsSuccess)
                {
                    var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                    httpResponseMessage.Content = new StreamContent(model.DownloadStream);
                    httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    httpResponseMessage.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                    httpResponseMessage.Content.Headers.Add("Access-Control-Expose-Headers", "Content-Disposition,X-Suggested-Filename");
                    httpResponseMessage.Content.Headers.ContentDisposition.FileName = string.Format(ApiConstants.BootstrapContainerFileName,
                        model.SerialNumber, date);
                    return ResponseMessage(httpResponseMessage);
                }
                else
                {
                    message = model.ErrorMessage;
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                message = Constants.UnexpectedError;
            }
            return this.Json(new { data = new { success = false, message = message } });
        }

        [HttpGet]
        [JwtTokenAuthentication]
        public IHttpActionResult GetPendingAssets(long userAccountId, long supportAccountId)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId))
            {
                return Unauthorized();
            }

            var response = new PendingDeviceTransferResponse();

            var userAccount = new UserAccount()
            {
                Id = userAccountId
            };

            try
            {
                var pendingTransfers = _deviceTransferLogic.GetReviewTransferData(userAccount, supportAccountId);
                response.PendingDeviceTransfers = Mapper.Map<IEnumerable<PendingDeviceTransferModel>>(pendingTransfers);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(response);
        }

        private bool IsAscUser(long userAccountId, long supportAccountId, string email)
        {
            IEnumerable<RoleSecurity> roleSecurities = new List<RoleSecurity>();
            if (string.IsNullOrEmpty(email))
            {
                email = _userAccountLogic.GetUserAccount(userAccountId, userAccountId).Email;
            }
            var response = _securityLogic.GetSessionDetails(email, supportAccountId);

            if (response.SecurityClassifications != null)
            {
                roleSecurities = (from sc in response.SecurityClassifications
                                  select new RoleSecurity
                                  {
                                      SecurityClassificationNum = sc.SecurityClassification,
                                      CategoryNum = sc.Category,
                                      SupportAccountId = sc.SupportAccountId,
                                      GroupId = sc.GroupId
                                  }).ToList();
            }
            return roleSecurities.Any(x => x.SecurityClassification == RoleSecurity.SecurityClassifications.Asc
                                               && x.Category == RoleSecurity.Categories.Account
                                               && x.SupportAccountId == supportAccountId);
        }

        [HttpGet]
        [JwtTokenAuthentication]
        public IHttpActionResult GetAddOnsAndSubscriptions(long userAccountId, long supportAccountId, string tenantId)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId))
            {
                return Unauthorized();
            }
            try
            {
                var result = _assetApiLogic.GetAddOnsAndSubscriptions(userAccountId, supportAccountId, tenantId);
                return this.Json(result);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(string.Empty);
        }

        [HttpPost]
        [JwtTokenAuthentication]
        public IHttpActionResult RegisterAsset([FromBody] BaseRequest request)
        {
            if (!JwtTokenManager.IsAuthorized(request.UserAccountId.ToString(), ClaimType.UserAccountId))
            {
                return Unauthorized();
            }
            try
            {
                // Sanitize input to prevent XSS attacks
                var sanitizedRequest = SanitizeBaseRequest(request);
                var result = _assetApiLogic.RegisterAsset(sanitizedRequest);
                return this.Json(result);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(string.Empty);
        }

        private BaseRequest SanitizeBaseRequest(BaseRequest request)
        {
            if (request == null) return null;

            const string escapePlus = @"&#43;";
            // Sanitize string properties that could contain user input
            request.SerialNumber = !string.IsNullOrEmpty(request.SerialNumber)
                  ? Sanitizer.GetSafeHtmlFragment(request.SerialNumber)
                  : request.SerialNumber;

            request.TenantId  = !string.IsNullOrEmpty(request.TenantId)
                 ? Sanitizer.GetSafeHtmlFragment(request.TenantId)
                 : request.TenantId;

            request.UserEmail = !string.IsNullOrEmpty(request.UserEmail)
                 ? Sanitizer.GetSafeHtmlFragment(request.UserEmail).Replace(escapePlus, "+")
                 : request.UserEmail;
            return request;
        }

        [HttpPost]
        [JwtTokenAuthentication]
        public IHttpActionResult DownloadAssetData(long userAccountId, [FromBody] AssetRequest asssetRequest)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, asssetRequest.SupportAccountId, ClaimType.UserAccountId) || !(userAccountId == asssetRequest.UserAccountId))
            {
                return Unauthorized();
            }
            try
            {
                var downloadData = _assetApiLogic.DownloadAssetData(asssetRequest);
                return this.Json(downloadData);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(string.Empty);
        }

        [HttpGet]
        [JwtTokenAuthentication]
        public IHttpActionResult DownloadAscAssetData(long userAccountId, long supportAccountId)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId))
            {
                return Unauthorized();
            }
            try
            {
                var downloadData = _assetApiLogic.DownloadAscAssetData(supportAccountId, userAccountId);
                return this.Json(downloadData);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(string.Empty);
        }
        [HttpPut]
        [JwtTokenAuthentication]
        public IHttpActionResult UpdateDeviceName([FromBody] AssetNameUpdateRequest request)
        {
            if (!JwtTokenManager.IsAuthorized(request.UserAccountId.ToString(), ClaimType.UserAccountId))
            {
                return Unauthorized();
            }
            try
            {
                // Sanitize input to prevent XSS attacks
                var sanitizedRequest = SanitizeAssetNameUpdateRequest(request);
                if (String.IsNullOrEmpty(sanitizedRequest.Name)) {
                    return this.Json(string.Empty);
                }
                var result = _assetApiLogic.UpdateDeviceName(sanitizedRequest);
                return this.Json(result);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(string.Empty);
        }

        private AssetNameUpdateRequest SanitizeAssetNameUpdateRequest(AssetNameUpdateRequest request)
        {
            if (request == null) return null;
            request.Name = !string.IsNullOrEmpty(request.Name)
                ? Sanitizer.GetSafeHtmlFragment(request.Name)
                : request.Name;
            request.SerialNumber = !string.IsNullOrEmpty(request.SerialNumber)
                 ? Sanitizer.GetSafeHtmlFragment(request.SerialNumber)
                 : request.SerialNumber;
            return request;
        }


        [HttpPut]
        [JwtTokenAuthentication]
        public IHttpActionResult UpdatePanOsVersion([FromBody] PanOsVersionUpdateRequest request)
        {
            if (!JwtTokenManager.IsAuthorized(request.UserAccountId.ToString(), ClaimType.UserAccountId))
            {
                return Unauthorized();
            }
            try
            {
                // Sanitize input to prevent XSS attacks
                var sanitizedRequest = SanitizePanOsVersionUpdateRequest(request);
                var result = _assetApiLogic.UpdatePanOsVersion(sanitizedRequest);
                return this.Json(result);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(string.Empty);
        }

        private PanOsVersionUpdateRequest SanitizePanOsVersionUpdateRequest(PanOsVersionUpdateRequest request)
        {
            if (request == null) return null;
            // Sanitize string properties that could contain user input
            request.PanOsVersion = !string.IsNullOrEmpty(request.PanOsVersion)
                ? Sanitizer.GetSafeHtmlFragment(request.PanOsVersion)
                : request.PanOsVersion;

            request.SerialNumber = !string.IsNullOrEmpty(request.SerialNumber)
                ? Sanitizer.GetSafeHtmlFragment(request.SerialNumber)
                : request.SerialNumber;
            return request;
        }


        [HttpPut]
        [JwtTokenAuthentication]
        public IHttpActionResult UpdateRemarks([FromBody] RemarksUpdateRequest request)
        {
            if (!JwtTokenManager.IsAuthorized(request.UserAccountId.ToString(), ClaimType.UserAccountId))
            {
                return Unauthorized();
            }
            try
            {
                // Sanitize input to prevent XSS attacks
                var sanitizedRequest = SanitizeRemarksUpdateRequest(request);
                if (String.IsNullOrEmpty(sanitizedRequest.Remarks)) {
                    return this.Json(string.Empty);
                }
                var result = _assetApiLogic.UpdateRemarks(sanitizedRequest);
                return this.Json(result);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(string.Empty);
        }

        private RemarksUpdateRequest SanitizeRemarksUpdateRequest(RemarksUpdateRequest request)
        {
            if (request == null) return null;
            request.Remarks = !string.IsNullOrEmpty(request.Remarks)
                ? Sanitizer.GetSafeHtmlFragment(request.Remarks)
                : request.Remarks;

            request.SerialNumber = !string.IsNullOrEmpty(request.SerialNumber)
                ? Sanitizer.GetSafeHtmlFragment(request.SerialNumber)
                : request.SerialNumber;
            return request;
        }


        [HttpPut]
        [JwtTokenAuthentication]
        public IHttpActionResult UpdateLocation([FromBody] LocationUpdateRequest request)
        {
            if (!JwtTokenManager.IsAuthorized(request.UserAccountId.ToString(), ClaimType.UserAccountId))
            {
                return Unauthorized();
            }
            try
            {
                // Sanitize input to prevent XSS attacks
                var sanitizedRequest = SanitizeLocationUpdateRequest(request);
                if (String.IsNullOrEmpty(sanitizedRequest.Location.Address1))
                {
                    return this.Json(string.Empty);

                }
                var result = _assetApiLogic.UpdateLocation(sanitizedRequest);
           
                return this.Json(result);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(string.Empty);
        }

        private LocationUpdateRequest SanitizeLocationUpdateRequest(LocationUpdateRequest request)
        {
            if (request == null) return null;

            // Sanitize string properties that could contain user input
            request.Location.City = !string.IsNullOrEmpty(request.Location.City)
                    ? Sanitizer.GetSafeHtmlFragment(request.Location.City)
                    : request.Location.City;

            request.Location.Address1 = !string.IsNullOrEmpty(request.Location.Address1)
            ? Sanitizer.GetSafeHtmlFragment(request.Location.Address1)
            : request.Location.Address1;

            request.Location.Address2 = !string.IsNullOrEmpty(request.Location.Address2)
            ? Sanitizer.GetSafeHtmlFragment(request.Location.Address2)
            : request.Location.Address2;

            request.SerialNumber = !string.IsNullOrEmpty(request.SerialNumber)
                ? Sanitizer.GetSafeHtmlFragment(request.SerialNumber)
                : request.SerialNumber;

            request.Location.Region = !string.IsNullOrEmpty(request.Location.Region)
               ? Sanitizer.GetSafeHtmlFragment(request.Location.Region)
               : request.Location.Region;

            request.Location.ZipCode = !string.IsNullOrEmpty(request.Location.ZipCode)
            ? Sanitizer.GetSafeHtmlFragment(request.Location.ZipCode)
            : request.Location.ZipCode;

            return request;
        }

        [HttpPut]
        [JwtTokenAuthentication]
        public IHttpActionResult UpdateAssetTags([FromBody] AssetTagsUpdateRequest request)
        {
            if (!JwtTokenManager.IsAuthorized(request.UserAccountId.ToString(), ClaimType.UserAccountId))
            {
                return Unauthorized();
            }
            try
            {
                // Sanitize input to prevent XSS attacks
                var sanitizedRequest = SanitizeAssetTagsUpdateRequest(request);
                var result = _assetApiLogic.UpdateAssetTags(sanitizedRequest);
                return this.Json(result);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(string.Empty);
        }

        private AssetTagsUpdateRequest SanitizeAssetTagsUpdateRequest(AssetTagsUpdateRequest request)
        {
            if (request == null) return null;
            // Sanitize string properties that could contain user input
            request.SerialNumber = !string.IsNullOrEmpty(request.SerialNumber)
                ? Sanitizer.GetSafeHtmlFragment(request.SerialNumber)
                : request.SerialNumber;
            request.AssetTagId = request.AssetTagId?.Select(tag =>
                 !string.IsNullOrEmpty(tag) ? Sanitizer.GetSafeHtmlFragment(tag) : tag).ToArray();

            return request;
        }

        [HttpPost]
        [JwtTokenAuthentication]
        public IHttpActionResult RegisterTenant([FromBody] BaseRequest request)
        {
            if (!JwtTokenManager.IsAuthorized(request.UserAccountId.ToString(), ClaimType.UserAccountId))
            {
                return Unauthorized();
            }
            try
            {
                var result = _assetApiLogic.RegisterTenant(request);
                return this.Json(result);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(string.Empty);
        }


        [HttpPatch]
        [JwtTokenAuthentication]
        public IHttpActionResult ChangetoSpare([FromBody] SKUChangetoSpareRequest request)
        {
            if (!JwtTokenManager.IsAuthorized(request.UserAccountId.ToString(), ClaimType.UserAccountId))
            {
                return Unauthorized();
            }
            try
            {
                var result = _assetApiLogic.ChangetoSpare(request);
                return this.Json(result);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(string.Empty);
        }


        [HttpGet]
        [JwtTokenAuthentication]
        public IHttpActionResult GetAssetMetaData(long userAccountId, long supportAccountId, string serialNumber)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId))
            {
                return Unauthorized();
            }
            try
            {
                var result = _assetApiLogic.GetAssetMetaData(userAccountId, supportAccountId, serialNumber);
                return this.Json(result);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(string.Empty);
        }

        [HttpGet]
        [JwtTokenAuthentication]
        public IHttpActionResult AssetDataInitialLoad(long userAccountId, long supportAccountId)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId))
            {
                return Unauthorized();
            }
            try
            {
                var result = _assetApiLogic.AssetDataInitialLoad(supportAccountId);
                return this.Json(result);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(string.Empty);
        }

        private HttpResponseMessage GetStringWebException(WebException ex)
        {
            string response = string.Empty;

            using (var reader = new StreamReader(ex.Response.GetResponseStream()))
            {
                string jsonResponse = reader.ReadToEnd();

                if (!string.IsNullOrEmpty(jsonResponse))
                {
                    response = jsonResponse.ToString();
                }
            }
            dynamic result = JsonConvert.DeserializeObject(response);
            if (result != null)
            {
                return Request.CreateErrorResponse(((HttpWebResponse)ex.Response).StatusCode, ex);
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, Constants.UnexpectedError);
            }
        }
    }
}
