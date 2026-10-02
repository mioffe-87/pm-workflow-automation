using Csp.Api.Model.Enums;
using Csp.Api.Model.LicenseManagement;
using Csp.Api.Services.Contracts;
using Csp.Web.Filters;
using Csp.Web.HelperClasses;
using Csp.Api.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace Csp.Web.Controllers
{
    [Route("api/[controller]")]
    public class LicenseManagementController : ApiController
    {
        private readonly ILicenseManagementLogic licenseManagementLogic;
        private readonly IMembershipLogic membershiplogic;
        private readonly ISecurityLogic _securityLogic;

        public LicenseManagementController(ILicenseManagementLogic licenseManagementLogic, IMembershipLogic membershiplogic, ISecurityLogic securityLogic)
        {
            this.licenseManagementLogic = licenseManagementLogic;
            this.membershiplogic = membershiplogic;
            this._securityLogic = securityLogic;
        }

        [JwtTokenAuthentication]
        [HttpPost, HttpOptions]
        public IHttpActionResult GetActivatedLicenses(long supportAccountId, long UserAccountId)
        {
            if (!JwtTokenManager.IsAuthorized(UserAccountId, supportAccountId, ClaimType.UserAccountId) || !_securityLogic.HasPermission(supportAccountId, UserAccountId, Permission.ViewSupportAccountDevices))
            {
                return Unauthorized();
            }

            var returnData = new List<LicenseManagementActivatedListItem>();

            try
            {
                returnData = licenseManagementLogic.GetActivatedLicenses(supportAccountId).ToList(); 
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(returnData);
        }

        [JwtTokenAuthentication]
        [HttpPost, HttpOptions]
        public IHttpActionResult GetAvailableLicenses(long supportAccountId, long UserAccountId)
        {
            if (!JwtTokenManager.IsAuthorized(UserAccountId, supportAccountId, ClaimType.UserAccountId) || !_securityLogic.HasPermission(supportAccountId, UserAccountId, Permission.ViewSupportAccountDevices))
            {
                return Unauthorized();
            }
            var returnData = new LicenseManagementAvailableData();
            try
            {
                returnData = licenseManagementLogic.GetAvailableLicenses(supportAccountId);                
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(returnData);
        }

        [JwtTokenAuthentication]
        [HttpPost, HttpOptions]
        public IHttpActionResult GetActivationLink(LicenseManagementActivationLinkRequest requestData)
        {
            var status = false;
            string message = string.Empty;
            if (!JwtTokenManager.IsAuthorized(requestData.UserAccountId, requestData.SupportAccountId, ClaimType.UserAccountId) || !_securityLogic.HasPermission(requestData.SupportAccountId, requestData.UserAccountId, Permission.ViewSupportAccountDevices))
            {
                return Unauthorized();
            }
            try
            {
                var activationLink = string.Empty;
                var result = licenseManagementLogic.GetActivationLink(requestData.AuthCodes, requestData.SupportAccountId);

                if (result != null && result.Success && result.Licenses != null)
                    activationLink = result.Licenses[0].ActivationLink;

                if (string.IsNullOrEmpty(activationLink))
                {
                    if (result != null) message = result.Message;
                }
                else
                {
                    status = true;
                    message = activationLink;
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(new { Success = status, Message = message });
        }
    }
}
