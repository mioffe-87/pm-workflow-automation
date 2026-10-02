using Csp.Api.HelperClasses;
using Csp.Api.Model;
using Csp.Api.Security;
using Csp.Api.Services.Contracts;
using Csp.Web.DataTransferObjects;
using Csp.Web.Filters;
using Csp.Web.HelperClasses;
using Csp.Web.Security.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Csp.Api.Model.Enums;

namespace Csp.Web.Controllers
{
    public class AssetLicenseApiController : ApiController
    {
        private readonly ILicenseLogic _licenseLogic;
        private readonly INetworkSecurityApiLogic _networkSecurityApiLogic;
        private readonly ISecurityLogic _securityLogic;

        public AssetLicenseApiController(ILicenseLogic licenseLogic, INetworkSecurityApiLogic networkSecurityApiLogic, ISecurityLogic securityLogic)
        {
            _licenseLogic = licenseLogic;
            _networkSecurityApiLogic = networkSecurityApiLogic;
            _securityLogic = securityLogic;
        }

        [HttpGet]
        [JwtTokenAuthentication]
        public IHttpActionResult DeactivateLicenses(string deactivationTokenText)
        {
            var status = false;
            try
            {
                status = _licenseLogic.DeactivateLicense(deactivationTokenText);
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
                    message = status ? ApiConstants.LicenseDeactivationSucessMessage :
                    ApiConstants.LicenseDeactivationFailedMessage
                }
            });
        }


        [HttpGet]
        [JwtTokenAuthentication]
        public IHttpActionResult GetTrialLicenses(long supportAccountId, long userAccountId, string serialNumber)
        {

            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId) || !_securityLogic.HasPermission(supportAccountId, userAccountId, Permission.EditDevice))
            {
                return Unauthorized();
            }
            try
            {
                var result = _networkSecurityApiLogic.GetTrialLicenses(supportAccountId, userAccountId, serialNumber);
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
        public IHttpActionResult ActivateTrialLicenses(long userAccountId, [FromBody]TrialLicenseRequest trialLicenseRequest)
        {

            if (!JwtTokenManager.IsAuthorized(userAccountId, trialLicenseRequest.SupportAccountId, ClaimType.UserAccountId) || !_securityLogic.HasPermission(trialLicenseRequest.SupportAccountId, userAccountId, Permission.EditDevice))
            {
                return Unauthorized();
            }

            try
            {
                var result = _networkSecurityApiLogic.ActivateTrialLicenses(trialLicenseRequest);
                return this.Json(result);
            }
            catch (WebException ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                var result = ExceptionHelper.GetWebException(ex);
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
        public IHttpActionResult ActivateLicense(long userAccountId, [FromBody]LicenseRequest licenseRequest)
        {

            if (!JwtTokenManager.IsAuthorized(userAccountId, licenseRequest.SupportAccountId, ClaimType.UserAccountId) || !_securityLogic.HasPermission(licenseRequest.SupportAccountId, userAccountId, Permission.EditDevice))
            {
                return Unauthorized();
            }
            try
            {
                var result = _networkSecurityApiLogic.ActivateLicense(licenseRequest);
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
        public IHttpActionResult GetLicenseActivationTypes(long userAccountId, long supportAccountId, string serialNumber)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId) || !_securityLogic.HasPermission(supportAccountId, userAccountId, Permission.EditDevice))
            {
                return Unauthorized();
            }
            try
            {
                var result = _networkSecurityApiLogic.GetLicenseActivationTypes(userAccountId, supportAccountId, serialNumber);
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
        public IHttpActionResult GetVirtualPlatforms(long userAccountId)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId.ToString(), ClaimType.UserAccountId))
            {
                return Unauthorized();
            }
            try
            {
                var result = _networkSecurityApiLogic.GetVirtualPlatforms();
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
        public IHttpActionResult ActivateOfflineLicense(long userAccountId, [FromBody]OfflineLicenseRequest licenseRequest)
        {

            if (!JwtTokenManager.IsAuthorized(userAccountId, licenseRequest.SupportAccountId, ClaimType.UserAccountId) || !_securityLogic.HasPermission(licenseRequest.SupportAccountId, userAccountId, Permission.EditDevice))
            {
                return Unauthorized();
            }
            try
            {
                var result = _networkSecurityApiLogic.ActivateOfflineLicense(licenseRequest);
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
        public IHttpActionResult GetFeatureLicenses(long userAccountId, long supportAccountId, string serialNumber)
        {

            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId) || !_securityLogic.HasPermission(supportAccountId, userAccountId, Permission.EditDevice))
            {
                return Unauthorized();
            }
            try
            {
                var result = _networkSecurityApiLogic.GetFeatureLicenses(userAccountId, supportAccountId, serialNumber);
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
        public IHttpActionResult RollbackVmUpgrade(long userAccountId, long supportAccountId, string serialNumber, string transactionId)
        {

            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId) || !_securityLogic.HasPermission(supportAccountId, userAccountId, Permission.EditDevice))
            {
                return Unauthorized();
            }
            try
            {
                var result = _networkSecurityApiLogic.RollbackVmUpgrade(userAccountId, supportAccountId, serialNumber, transactionId);
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
        public IHttpActionResult GetCompatibleDevices(long userAccountId, long supportAccountId, string serialNumber, bool isRMAed = false)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId) || !_securityLogic.HasPermission(supportAccountId, userAccountId, Permission.EditDevice))
            {
                return Unauthorized();
            }
            try
            {
                var result = _networkSecurityApiLogic.GetDevicesForLicenseTransfer(userAccountId, supportAccountId, serialNumber, isRMAed);
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
        public IHttpActionResult TransferAssetLicense( [FromBody]TransferAssetLicenseRequest transferAssetLicenseRequest)
        {
            if (!JwtTokenManager.IsAuthorized(transferAssetLicenseRequest.UserAccountId, transferAssetLicenseRequest.SupportAccountId, ClaimType.UserAccountId) || !_securityLogic.HasPermission(transferAssetLicenseRequest.SupportAccountId, transferAssetLicenseRequest.UserAccountId, Permission.EditDevice))
            {
                return Unauthorized();
            }
            try
            {
                var result = _networkSecurityApiLogic.TransferAssetLicense(transferAssetLicenseRequest);
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
        public IHttpActionResult ActivateSiteLicense([FromBody]ActivateSiteLicenseRequest activateSiteLicenseRequest)
        {
            if (!JwtTokenManager.IsAuthorized(activateSiteLicenseRequest.UserAccountId, activateSiteLicenseRequest.SupportAccountId, ClaimType.UserAccountId) || !_securityLogic.HasPermission(activateSiteLicenseRequest.SupportAccountId, activateSiteLicenseRequest.UserAccountId, Permission.EditDevice))
            {
                return Unauthorized();
            }
            try
            {
                var result = _networkSecurityApiLogic.ActivateSiteLicense(activateSiteLicenseRequest);
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
        public IHttpActionResult GetAssetLicense(long userAccountId, long supportAccountId, long categoryId, string serialNumber)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId) || !_securityLogic.HasPermission(supportAccountId, userAccountId, Permission.EditDevice))
            {
                return Unauthorized();
            }
            try
            {
                var result = _networkSecurityApiLogic.GetAssetLicense(userAccountId, supportAccountId, categoryId, serialNumber);
                return this.Json(result);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(string.Empty);
        }

        [HttpPost, HttpOptions]
        [JwtTokenAuthentication]
        public IHttpActionResult DeleteExpiredTrialLicenses([FromBody]DeleteTrialLicenseRequest details)
        {
            if (!JwtTokenManager.IsAuthorized(details?.UserAccountId?.ToString(), ClaimType.UserAccountId))
            {
                return Unauthorized();
            }
            try
            {
                var result = _networkSecurityApiLogic.DeleteTrialLicenses(details);
                return this.Json(result);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(string.Empty);
        }
    }
}
