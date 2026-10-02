using AutoMapper;
using Csp.Api.Security;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;
using System.Text;
using Csp.Api.HelperClasses;
using System.Threading.Tasks;
using System.Linq;
using Csp.Web.Filters;
using System.Web.Http;
using Csp.Api.Services.Contracts;
using Csp.Web.HelperClasses;
using Csp.Api.Model.Enums;
using System;
using Csp.Api.Model.Flex;
using System.IO;
using Csp.Api.Model;
using Csp.Api;

namespace Csp.Web.Controllers
{
    [JwtTokenAuthentication]
    public class FirewallFlexApiController : ApiController
    {
        private readonly IFirewallFlexApiLogic _flexApiLogic;
        private readonly ISecurityLogic _securityLogic;
        public FirewallFlexApiController(IFirewallFlexApiLogic flexApiLogic, ISecurityLogic securityLogic)
        {
            _flexApiLogic = flexApiLogic;
            _securityLogic = securityLogic;
        }

        [HttpPost]
        [JwtTokenAuthentication]
        public IHttpActionResult GetCNFirewallSDevices([FromBody] FlexDeviceRequest flexDeviceRequest, long userAccountId, long supportAccountId, string type, bool export = false, string subType = null)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId) || !(HasFirewallFlexAccess(supportAccountId, userAccountId)))
            {
                return Unauthorized();
            }
            try
            {
                var result = _flexApiLogic.GetCNFirewallSDevices(flexDeviceRequest, userAccountId, supportAccountId, type, export, subType);
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
                        FileName = "export.csv"
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
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(string.Empty);
        }

        [HttpGet]
        public IHttpActionResult DownloadAirgapLicense(long supportAccountId, long userAccountId, string authCode, bool withPanorama)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId) || !(HasFirewallFlexAccess(supportAccountId, userAccountId)))
            {
                return Unauthorized();
            }

            try
            {
                var result = _flexApiLogic.DownloadAirgapLicense(supportAccountId, userAccountId, authCode, withPanorama);
                result.Content.Seek(0, SeekOrigin.Begin);
                var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                httpResponseMessage.Content = new StreamContent(result.Content);
                httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                httpResponseMessage.Content.Headers.Add("Access-Control-Expose-Headers", "Content-Disposition,X-Suggested-Filename");
                httpResponseMessage.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = result.FileName
                };

                return ResponseMessage(httpResponseMessage);

            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                var httpResponse = Request.CreateErrorResponse(HttpStatusCode.NotFound, "An unexpected error has occurred.");
                return ResponseMessage(httpResponse);
            }
        }

        [HttpPost]
        public IHttpActionResult RetryFailedDeploymentProfile(RetryFailedDPRequest retryFailedDPRequest)
        {
            if (!JwtTokenManager.IsAuthorized(retryFailedDPRequest.UserAccountId, retryFailedDPRequest.SupportAccountId, ClaimType.UserAccountId) || !(HasFirewallFlexAccess(retryFailedDPRequest.SupportAccountId, retryFailedDPRequest.UserAccountId)))
            {
                return Unauthorized();
            }

             try
             {
                    var response = _flexApiLogic.RetryFailedDeploymentProfile(retryFailedDPRequest.Id);
                    return Json(response);
             }
                            
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                var httpResponse = Request.CreateErrorResponse(HttpStatusCode.NotFound, "An unexpected error has occurred.");
                return ResponseMessage(httpResponse);
            }
        }

        [HttpPost]
        [JwtTokenAuthentication]
        public IHttpActionResult CreateFirewallFlexDeploymentProfile([FromBody]FirewallFlexDeploymentProfile firewallFlexDeploymentProfile)
        {

            if (!JwtTokenManager.IsAuthorized(firewallFlexDeploymentProfile.UserAccountId, firewallFlexDeploymentProfile.SupportAccountId, ClaimType.UserAccountId))
            {
                return Unauthorized();
            }

            try
            {
                var result = _flexApiLogic.CreateFirewallFlexDeploymentProfile(firewallFlexDeploymentProfile);
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
        public IHttpActionResult UpdateFirewallFlexDeploymentProfile([FromBody]FirewallFlexDeploymentProfile firewallFlexDeploymentProfile)
        {

            if (!JwtTokenManager.IsAuthorized(firewallFlexDeploymentProfile.UserAccountId, firewallFlexDeploymentProfile.SupportAccountId, ClaimType.UserAccountId))
            {
                return Unauthorized();
            }

            try
            {
                var result = _flexApiLogic.UpdateFirewallFlexDeploymentProfile(firewallFlexDeploymentProfile);
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
        public IHttpActionResult GetEstimatedCost([FromBody]Api.TokenCalculatorRequest tokenCalculatorRequest, long supportAccountId, long userAccountId)
        {

            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId))
            {
                return Unauthorized();
            }

            try
            {
                var result = _flexApiLogic.GetEstimatedCost(tokenCalculatorRequest, supportAccountId, userAccountId);
                return this.Json(result);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(string.Empty);
        }

        private bool HasFirewallFlexAccess(long supportAccountId, long userAccountId, bool hasAdminAccess = false)
        {
            var userAccount = new UserAccount()
            {
                Id = userAccountId
            };

            var userPermissions = _securityLogic.GetSupportAccountPermissionsForPrincipal(
                                    supportAccountId, userAccount);
            bool isAuthorized = hasAdminAccess ? userPermissions.Contains(Permission.ManageFirewallFlexTokens) :
                                                    userPermissions.Contains(Permission.ViewFirewallFlexTokens);

            return isAuthorized;
        }
    
        
    }
}
