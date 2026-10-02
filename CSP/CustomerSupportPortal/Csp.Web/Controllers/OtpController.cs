using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Http;
using Csp.Api.Model;
using Csp.Api.Model.Enums;
using Csp.Api.Services.Contracts;
using Csp.Web.Filters;
using Csp.Web.HelperClasses;
using Newtonsoft.Json;
using System.Net;
using Csp.Web.Security.Attributes;
using Csp.Api.Security;
using System.Net.Http;
using System.Net.Http.Headers;
using Csp.Api.Logic;
using System.Configuration;
using System.Globalization;

namespace Csp.Web.Controllers
{
    public class OtpController : ApiController
    {
        #region Constants
        public const string DEVICE_SELECTION_ERRMESSAGE = "Please select the serial number for OTP generation.";
        public const string UNEXPECTED_ERRMESSAGE = "An unexpected error has occurred. Please try again or contact support.";
        public const string CORRUPT_BLOB_ERRMESSAGE = "Unable to decode the OTP request.";
       
        #endregion

        private readonly ISaasLogic _saasLogic;
        private readonly IOtpLogic _otpLogic;
        private readonly IDeviceLogic _deviceLogic;
        private readonly ISecurityLogic _securityLogic;

        public OtpController(IOtpLogic otpLogic, ISaasLogic saasLogic, IDeviceLogic deviceLogic, ISecurityLogic securityLogic)
        {
            _otpLogic = otpLogic;
            _saasLogic = saasLogic;
            _deviceLogic = deviceLogic;
            _securityLogic = securityLogic;
        }

        #region One Time Password

        [HttpPost]
        [JwtTokenAuthentication]
        public IHttpActionResult GetDeviceRegistrationOtpSummary(RequestParameters request, long supportAccountId, long userAccountId, bool showHistory = false)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId) || !_securityLogic.HasPermission(supportAccountId, userAccountId, Permission.ViewDeviceCertificates))
            {
                return Unauthorized();
            }

            var dataParameters = RequestParameters.ToDataParameter(request);

            var otps = new OtpSummary<OtpSummaryItem>();
            try
            {
                otps = _otpLogic.GetDeviceRegistrationOtpSummary(supportAccountId, userAccountId, dataParameters, showHistory);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return Json(otps);
        }

        [HttpGet]
        [JwtTokenAuthentication]
        public IHttpActionResult GetDeviceSerialNumbers(long supportAccountId, long userAccountId, OtpDeviceType deviceType)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId) || !_securityLogic.HasPermission(supportAccountId, userAccountId, Permission.ViewDeviceCertificates))
            {
                return Unauthorized();
            }

            var serialNumbers = new List<NameValueOnly>();

            try
            {
                if (!deviceType.Equals(OtpDeviceType.BulkPanOS))
                {
                    serialNumbers = _otpLogic.GetDevicesForOtp(supportAccountId, userAccountId, deviceType);
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return Json(serialNumbers);
        }

        [HttpPost, HttpOptions]
        [JwtTokenAuthentication]
        public IHttpActionResult GenerateOtp(GenerateOtpModel requestData)
        {
            if (!JwtTokenManager.IsAuthorized(requestData.UserAccountId, requestData.SupportAccountId, ClaimType.UserAccountId) || !_securityLogic.HasPermission(requestData.SupportAccountId, requestData.UserAccountId, Permission.ViewDeviceCertificates))
            {
                return Unauthorized();
            }

            var returnData = new GenerateOtpResult();

            try
            {
                //var isUserAuthorized = _saasLogic.VerifyAuthorizationBySupportAccount(requestData.SupportAccountId, requestData.UserEmail);
                if (requestData.SupportAccountId != 0) // && isUserAuthorized)
                {
                    OTPOutput otpOutput;
                    //Validate if the SN is registered in the same support account which is provided in the request
                    if (!string.IsNullOrEmpty(requestData.SerialNumber))
                     {
                        long supportAccountId = _deviceLogic.GetSupportAccountIdByDeviceBySerialNumber(requestData.SerialNumber);
                        if (requestData.SupportAccountId != supportAccountId) {
                            var ex = new UnauthorizedAccessException("User does not have permission to generate OTP for the given SerialNumber");
                            Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                            throw ex;
                        }
                    }

                    //Generate OTP for a single device
                    if (!string.IsNullOrEmpty(requestData.SerialNumber))
                    {


                        var otp = new SingleOtpRequest
                        {
                            SerialNumber = requestData.SerialNumber,
                            SupportAccountId = requestData.SupportAccountId,
                            OtpType = requestData.OtpType.ToString(),
                            UpdatedBy = requestData.UserAccountId.ToString(),
                            DeviceType = requestData.DeviceType.ToString()
                        };
                       
                        var response = _otpLogic.GenerateSingleOtp(otp);
                        returnData = response.ResponseObject;
                        returnData.ExpirationDate = DateTime.Parse(returnData.ExpirationDate).ToString("G", CultureInfo.CreateSpecificCulture("en-us"));

                    }

                       
                    
                    //Generate Bulk OTP for the devices managed by Panorama
                    else if (requestData.DeviceType.Equals(OtpDeviceType.BulkPanOS) &&
                             !string.IsNullOrEmpty(requestData.BulkOtpRequest))
                    {

                        BulkOtpRequest bulkOtprequest = new BulkOtpRequest()
                        {
                            UserAccountId = requestData.UserAccountId,
                            SupportAccountId = requestData.SupportAccountId,
                            HashCode = requestData.BulkOtpRequest
                        };


                        var response = _otpLogic.GenerateBulkOtp(bulkOtprequest);
                        if (response == null)
                        {
                            returnData.IsSuccess = true;
                            returnData.IsBulkOtp = true;
                        }

                        else
                        {
                            returnData.IsSuccess = false;
                            returnData.Message = UNEXPECTED_ERRMESSAGE;
                        }

                    }
                    else
                    {
                        returnData.IsSuccess = false;
                        returnData.Message = DEVICE_SELECTION_ERRMESSAGE;
                    }

                }
                else
                {
                    returnData.IsSuccess = false;
                    returnData.Message = UNEXPECTED_ERRMESSAGE;
                }
            }
            catch (JsonReaderException jex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(jex);
                returnData.IsSuccess = false;
                returnData.Message = CORRUPT_BLOB_ERRMESSAGE;
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                returnData.IsSuccess = false;
                returnData.Message = UNEXPECTED_ERRMESSAGE;
            }
            return Json(returnData);
        }

        [HttpGet]
        [JwtTokenAuthentication]
        public IHttpActionResult DownloadOtpHistory(long supportAccountId, long userAccountId)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId) || !_securityLogic.HasPermission(supportAccountId, userAccountId, Permission.ViewDeviceCertificates))
            {
                return Unauthorized();
            }

            var response = _otpLogic.GetOtpHistoryCsv(supportAccountId, userAccountId);
            if (response != null)
            {
                var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                httpResponseMessage.Content = new ByteArrayContent(response);
                httpResponseMessage.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                httpResponseMessage.Content.Headers.Add("Access-Control-Expose-Headers", "Content-Disposition,X-Suggested-Filename");
                httpResponseMessage.Content.Headers.ContentDisposition.FileName = string.Format("OtpHistory_{0}.csv", supportAccountId);
                return ResponseMessage(httpResponseMessage);
            }
            else
            {
                var httpResponse = Request.CreateErrorResponse(HttpStatusCode.NotFound, "An unexpected error has occurred.");
                return ResponseMessage(httpResponse);
            }
        }

        #endregion

        #region Device Registration Pin

        [HttpGet]
        [JwtTokenAuthentication]
        public IHttpActionResult GetDeviceRegistrationPinSummary(long supportAccountId, long userAccountId, int pageNumber = 1, int perPage = int.MaxValue)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId) || !_securityLogic.HasPermission(supportAccountId, userAccountId, Permission.ViewDeviceCertificates))
            {
                return Unauthorized();
            }

            //var dataParameters = request.ToDataParameters(searchField, searchValue);
            var dataParameters = new DataParameters { PaginationCount = perPage, PaginationOffset = pageNumber };
            var otps = new OtpSummary<OtpPinSummaryItem>();
            try
            {
                otps = _otpLogic.GetDeviceRegistrationPinSummary(supportAccountId, userAccountId, dataParameters);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return Json(otps);
        }

        [HttpPost, HttpOptions]
        [JwtTokenAuthentication]
        public void DisableDeviceRegistrationPin(DisableDeviceRegistrationPinModel requestData)
        {
            if (!JwtTokenManager.IsAuthorized(requestData.UserAccountId, requestData.SupportAccountId, ClaimType.UserAccountId) || !_securityLogic.HasPermission(requestData.SupportAccountId, requestData.UserAccountId, Permission.ViewDeviceCertificates))
            {
                throw new HttpResponseException(HttpStatusCode.Unauthorized);
            }

            try
            {
                _otpLogic.DisableDeviceRegistrationPin(requestData.SupportAccountId, requestData.UserAccountId, requestData.Ids);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
        }

        [HttpPost, HttpOptions]
        [JwtTokenAuthentication]
        public IHttpActionResult GenerateRegistrationPin(GenerateRegistrationPinRequest requestData)
        {
            if (!JwtTokenManager.IsAuthorized(requestData.UserAccountId, requestData.SupportAccountId, ClaimType.UserAccountId) || !_securityLogic.HasPermission(requestData.SupportAccountId, requestData.UserAccountId, Permission.ViewDeviceCertificates))
            {
                throw new HttpResponseException(HttpStatusCode.Unauthorized);
            }

            var returnData = new GenerateRegistrationPinResponse();

            try
            {
                if (requestData.PinExpirationInDays > 365)
                {
                    returnData.IsSuccess = false;
                    returnData.Message = "Pin expiration cannot exceed 365 days.";
                    return Json(returnData);
                }

                //var isUserAuthorized = _saasLogic.VerifyAuthorizationBySupportAccount(requestData.SupportAccountId, requestData.UserEmail);
                if (requestData.SupportAccountId != 0) // && isUserAuthorized)
                {
                    var registrationPin = new RegistrationPinModel
                    {
                        SupportAccountId = requestData.SupportAccountId,
                        RegistrationPinUniqueId = Guid.NewGuid().ToString(),
                        RegistrationPin = Guid.NewGuid().ToString().Replace("-",""),
                        Description = requestData.Description,
                        ExpirationDate = DateTime.Now.AddDays(requestData.PinExpirationInDays),
                        IsActive = true,
                        CreatedBy = requestData.UserAccountId
                    };

                    if (_otpLogic.SaveRegistrationPin(registrationPin))
                    {
                        returnData.IsSuccess = true;
                        returnData.Message = string.Empty;
                        returnData.RegistrationPinId = registrationPin.RegistrationPinUniqueId;
                        returnData.RegistrationPin = registrationPin.RegistrationPin;
                        returnData.ExpirationDate = registrationPin.ExpirationDate.ToShortDateString();
                    }
                    else
                    {
                        returnData.IsSuccess = false;
                        returnData.Message = UNEXPECTED_ERRMESSAGE;
                    }
                }
                else
                {
                    returnData.IsSuccess = false;
                    returnData.Message = UNEXPECTED_ERRMESSAGE;
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                returnData.IsSuccess = false;
                returnData.Message = UNEXPECTED_ERRMESSAGE;
            }
            return Json(returnData);
        }

        [HttpPost]
        public IHttpActionResult GenerateOtpRequest(GenerateOtpModel requestData)
        {
            if (!_otpLogic.EnableThermiteApiTest())
            {
                throw new WebException("GenerateOtpRequest is for testing purpose only and disabled in production", WebExceptionStatus.RequestProhibitedByProxy);
            }
            return GenerateOtp(requestData);
        }

        #endregion
    }
}
