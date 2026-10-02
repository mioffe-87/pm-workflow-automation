using System;
using System.Collections.Generic;
using System.Web.Http;
using Csp.Api.Model;
using Csp.Api.Model.Enums;
using Csp.Web.Filters;
using Csp.Web.HelperClasses;
using Csp.Api.Services.Contracts;
using Csp.Api.Model.Flex;
using AutoMapper;
using Csp.Api.Security;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;
using System.Text;
using Csp.Api.HelperClasses;
using System.Threading.Tasks;
using System.Linq;
using Csp.Web.Security.Attributes;
using Newtonsoft.Json;

namespace Csp.Web.Controllers
{
    [JwtTokenAuthentication]
    public class FirewallFlexController : ApiController
    {
        private readonly IFirewallFlexDataAccess _firewallFlexDataAccess;
        private readonly IFirewallFlexLogic _flexLogic;
        private readonly ISecurityLogic _securityLogic;
        private readonly ISoftwareUpdateLogic _softwareUpdateLogic;
        private readonly IDeviceLogic _deviceLogic;
        private readonly INetworkSecurityApiLogic _networkSecurityApiLogic;
        public FirewallFlexController(IFirewallFlexLogic flexLogic, IFirewallFlexDataAccess firewallFlexDataAccess, ISecurityLogic securityLogic, ISoftwareUpdateLogic softwareUpdateLogic, IDeviceLogic deviceLogic, INetworkSecurityApiLogic networkSecurityApiLogic)
        {
            _flexLogic = flexLogic;
            _firewallFlexDataAccess = firewallFlexDataAccess;
            _securityLogic = securityLogic;
            _softwareUpdateLogic = softwareUpdateLogic;
            _deviceLogic = deviceLogic;
            _networkSecurityApiLogic = networkSecurityApiLogic;
        }

        [HttpGet]
        public IHttpActionResult GetUseCaseSubscriptions(long supportAccountId, string type, string productId, string genType = "GEN-1", string panOs = null, string subType = null)
        {
            UseCaseSubscription useCaseSubscriptionMapping = null;
            try
            {
                DeploymentProfileType deploymentProfileType = (DeploymentProfileType)Enum.Parse(typeof(DeploymentProfileType), type, true);
                EDeploymentProfileSubType? deploymentProfileSubTypeId = !string.IsNullOrWhiteSpace(subType) ? (EDeploymentProfileSubType?)Enum.Parse(typeof(EDeploymentProfileSubType), subType, true) : null;
                useCaseSubscriptionMapping = _firewallFlexDataAccess.GetUseCaseSubscriptions(supportAccountId, deploymentProfileType, deploymentProfileSubTypeId, panOs, productId, genType);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return Ok(useCaseSubscriptionMapping);
        }

        [HttpGet]
        public IHttpActionResult CreditDetails(long userAccountId, string tokenIds)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId.ToString(), ClaimType.UserAccountId))
            {
                return Unauthorized();
            }
            try
            {
                return Json(_flexLogic.GetCreditDetails(tokenIds, userAccountId));
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new CreditDataModel[0]);
            }
        }

        [HttpGet]
        public IHttpActionResult CreditDetails(long supportAccountId, long userAccountId)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId) || !HasFirewallFlexAccess(supportAccountId, userAccountId))
            {
                return Unauthorized();
            }
            try
            {
                return Json(_flexLogic.GetCreditDetails(supportAccountId, userAccountId));
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new CreditDataModel[0]);
            }
        }

        [HttpGet]
        public IHttpActionResult SupportAccountsLicenses(long userAccountId)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId.ToString(), ClaimType.UserAccountId))
            {
                return Unauthorized();
            }
            try
            {
                return Json(_flexLogic.GetSupportAccountsLicenses(userAccountId));
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new SupportAccountLicense[0]);
            }
        }

        [HttpPost, HttpOptions]
        public IHttpActionResult DepositCredits(DepositCreditModel model)
        {
            if (!JwtTokenManager.IsAuthorized(model.UserAccountId, model.SupportAccountId, ClaimType.UserAccountId))
            {
                return Unauthorized();
            }

            var requestList = new List<DepositCreditRequest>();

            var supportAccountId = model.SupportAccountId;
            var userAccountId = model.UserAccountId;
            var transactionId = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            var status = false;

            try
            {
                if (model.CreditAssociations.Any(i => i.ProductId == ApiConstants.CLOUD_NGFW_PRODUCT_ID)) // CLOUD NGFW calls Pubsub
                {
                    status = _flexLogic.DepositCredits(model);
                }
                else
                {
                    foreach (var association in model.CreditAssociations)
                    {
                        var request = new DepositCreditRequest()
                        {
                            SupportAccountId = model.SupportAccountId,
                            TokenId = association.TokenId,
                            ProductId = association.ProductId,
                            CrossRefTransactionId = transactionId,
                            Quantity = association.TotalPurchased,
                            TransactionDate = DateTime.UtcNow
                        };
                        requestList.Add(request);
                    }

                    status = _flexLogic.DepositCredits(requestList, supportAccountId, userAccountId);
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(new { success = status, message = status ? Constants.TokenActivationSuccessMessage : Constants.TokenActivationFailedMessage });
        }

        public IHttpActionResult EditPoolName(PoolEditReq request, long userAccountId)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, request.SupportAccountId, ClaimType.UserAccountId))
            {
                return Unauthorized();
            }

            var status = false;
            try
            {
                status= _flexLogic.EditPoolName(request, userAccountId);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(new { success = status, message = status ? Constants.TokenPoolEditSuccessMessage : Constants.TokenPoolEditFailedMessage });
        }

        [HttpPost]
        public IHttpActionResult EstimatedCost(TokenCalculatorRequest request)
        {
            TokenCalculatorResponse response = null;
            try
            {
                response = _flexLogic.GetEstimatedCost(request);
                if (response != null)
                {
                    return Ok(response);
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return Ok(new { success = false, message = Constants.EstimateCostNotWorkingMessage });
        }

        [HttpPost, HttpOptions]
        public IHttpActionResult CreditTransfer(CreditTransferDataModel model)
        {
            if (!JwtTokenManager.IsAuthorized(model.UserAccountId, model.SourceSupportAccountId, ClaimType.UserAccountId)
                || !(HasFirewallFlexAccess(model.SourceSupportAccountId, model.UserAccountId, true)) || (model.DestinationSupportAccountId.HasValue &&
                model.SourceSupportAccountId != model.DestinationSupportAccountId
                && !(HasFirewallFlexAccess(model.DestinationSupportAccountId.Value, model.UserAccountId))))
            {
                return Unauthorized();
            }

            var request = new CreditTransferRequest();
            var status = false;

            try
            {
                if (model != null && model.SourceTokenId > 0)
                {
                    model.TransactionDate = DateTime.UtcNow;
                    model.CrossRefTransactionId = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
                    model.UsageDescription = FwFlexUsageDescription.Transfer.ToString();
                }
                Mapper.Map(model, request);
                status = _flexLogic.CreditTransfer(request);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(new
            {
                success = status,
                message = status ? Constants.CreditTransferredSuccessMessage : Constants.CreditTransferredFailedMessage
            });
        }

        [HttpGet]
        public IHttpActionResult ExpiredDeploymentProfiles(long supportAccountId, long userAccountId, long tokenId)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId) || !(HasFirewallFlexAccess(supportAccountId, userAccountId)))
            {
                return Unauthorized();
            }
            try
            {
                return Json(_flexLogic.GetExpiredDeploymentProfiles(supportAccountId, userAccountId, tokenId));
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new DeploymentProfile[0]);
            }
        }

        [HttpPost]
        [Obsolete]
        public IHttpActionResult AddUpdateDeploymentProfile(DeploymentProfile deploymentProfile)
        {
            if (!JwtTokenManager.IsAuthorized(deploymentProfile.CreatedBy, deploymentProfile.SupportAccountId, ClaimType.UserAccountId)
               || !(HasFirewallFlexAccess(deploymentProfile.SupportAccountId, deploymentProfile.CreatedBy)))
            {
                return Unauthorized();
            }

            DeploymentProfileStatus deploymentProfileStatus = null;
            string message = string.Empty;
            try
            {
                message = ValidateDeploymentProfile(deploymentProfile);
                if (string.IsNullOrEmpty(message))
                {
                    return Json(_flexLogic.AddUpdateDeploymentProfile(deploymentProfile));
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                message = ApiConstants.DeploymentProfileFailedMessage;
            }

            deploymentProfileStatus = new DeploymentProfileStatus
            {
                Success = false,
                Message = message
            };
            return Json(deploymentProfileStatus);
        }

        [HttpPost]
        public IHttpActionResult AvailableCreditTypes(CreditTypeRequest creditTypeRequest)
        {
            if (!JwtTokenManager.IsAuthorized(creditTypeRequest.UserAccountId, creditTypeRequest.SourceSupportAccountId, ClaimType.UserAccountId)
                || !(HasFirewallFlexAccess(creditTypeRequest.SourceSupportAccountId, creditTypeRequest.UserAccountId)))
            {
                return Unauthorized();
            }
            try
            {
                return Json(_flexLogic.GetAvailableCreditTypes(creditTypeRequest.SourceSupportAccountId, creditTypeRequest.TargetSupportAccountId,
                                                               creditTypeRequest.SourceTokenId, creditTypeRequest.UserAccountId));
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new CreditDataModel[0]);
            }
        }

        [HttpDelete]
        public IHttpActionResult DeleteDeploymentProfile(long id, long userAccountId, long supportAccountId, string productId, long tokenId, string type, string panOs = null, string subType = null)
        {
            string authCode = string.Empty;
            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId) || !(HasFirewallFlexAccess(supportAccountId, userAccountId)))
            {
                return Unauthorized();
            }

            DeploymentProfileModel response = null;
            try
            {
                authCode = _firewallFlexDataAccess.GetAuthCodeFromDeploymentProfileId(id, supportAccountId);
                if (string.IsNullOrEmpty(authCode))
                {
                    return Unauthorized();
                }

                if (!_firewallFlexDataAccess.IsUserHasPermissionToDelete(authCode, userAccountId, supportAccountId))
                {
                    return Unauthorized();
                }
                response = _flexLogic.DeleteDeploymentProfile(userAccountId, authCode, productId, tokenId, type, panOs, subType);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, response));
            }
            return Json(response);
        }

        [HttpPost]
        public IHttpActionResult Devices([FromBody] FlexDeviceRequest flexDeviceRequest, long supportAccountId, long userAccountId, string type, bool export = false, string subType = null)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId) || !(HasFirewallFlexAccess(supportAccountId, userAccountId)))
            {
                return Unauthorized();
            }
            try
            {
                var devices = _flexLogic.GetFlexDevices(flexDeviceRequest, supportAccountId, userAccountId, type, subType);

                if (export == true)
                {
                    bool addLastCheckinDate = false;
                    if (flexDeviceRequest.requestParameters.SearchField == "LastCheckinDate")
                    {
                        addLastCheckinDate = true;
                        
                    }
                   
                var csvData = GetExportToCsvData(devices, type, addLastCheckinDate);
                    var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                    httpResponseMessage.Content = new StringContent(csvData, Encoding.UTF8);
                    httpResponseMessage.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                    httpResponseMessage.Content.Headers.Add("Access-Control-Expose-Headers", "Content-Disposition,X-Suggested-Filename");
                    httpResponseMessage.Content.Headers.ContentDisposition.FileName = "export.csv";
                    return ResponseMessage(httpResponseMessage);
                }
                else
                {
                    return Json(devices);
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new FlexDevice[0]);
            }
        }

        [HttpPost]
        public IHttpActionResult Panoramas([FromBody] FlexDeviceRequest flexDeviceRequest, long supportAccountId, long userAccountId, bool export = false)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId) || !(HasFirewallFlexAccess(supportAccountId, userAccountId)))
            {
                return Unauthorized();
            }
            try
            {
                var panoramas = _flexLogic.GetPanoramas(flexDeviceRequest, supportAccountId);
                if (export == true)
                {
                    var csvData = new StringBuilder();
                    csvData.Append("Serial Number,Panorama Model,License,Auth Code,Expiration Date,Deployment Profile,Credit Pool Id\n");
                    panoramas.PanoromaDevices.panoramas.ForEach(x =>
                    {
                        csvData.AppendFormat("{0},{1},{2},{3},{4},{5},{6}\n",
                            x.SerialNumber,
                            x.PanoramaModel,                            
                           "\"" + string.Join("\n", x.Licenses.Select(y => y.Subscription)) + "\"",
                            x.AuthorizationCode,
                            x.ExpirationDate,
                            x.DeploymentProfile,
                            x.CreditPoolId);
                    });
                    var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                    httpResponseMessage.Content = new StringContent(csvData.ToString(), Encoding.UTF8);
                    httpResponseMessage.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                    httpResponseMessage.Content.Headers.Add("Access-Control-Expose-Headers", "Content-Disposition,X-Suggested-Filename");
                    httpResponseMessage.Content.Headers.ContentDisposition.FileName = "export.csv";
                    return ResponseMessage(httpResponseMessage);
                }
                else
                {
                    return Ok(panoramas);
                }
                
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new PanoramaDevice[0]);
            }
        }

        [HttpGet]
        public IHttpActionResult DeploymentProfileList(long userAccountId, long supportAccountId, long tokenId, string poolExpirationDate)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId) || !(HasFirewallFlexAccess(supportAccountId, userAccountId)))
            {
                return Unauthorized();
            }

            DeploymentProfileList response = null;
            try
            {
                response = _flexLogic.GetDeploymentProfiles(userAccountId, supportAccountId, tokenId, poolExpirationDate);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return Ok(response);
        }

        [HttpGet]
        public IHttpActionResult GetRenewalDeploymentProfiles(long userAccountId, long supportAccountId, long tokenId)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId) || !(HasFirewallFlexAccess(supportAccountId, userAccountId)))
            {
                return Unauthorized();
            }

            DeploymentProfileList response = null;
            try
            {
                response = _flexLogic.GetRenewalDeploymentProfiles(userAccountId, supportAccountId, tokenId);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return Ok(response);
        }

        [HttpGet]
        public IHttpActionResult GetDeploymentProfile(long deploymentProfileId, long supportAccountId, long userAccountId)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId) || !(HasFirewallFlexAccess(supportAccountId, userAccountId)))
            {
                return Unauthorized();
            }
            try
            {
                return Json(_flexLogic.GetDeploymentProfile(deploymentProfileId));
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new DeploymentProfile[0]);
            }
        }

        [HttpGet]
        public IHttpActionResult TransactionHistory(long userAccountId, long supportAccountId, string tokenId)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId) || !(HasFirewallFlexAccess(supportAccountId, userAccountId)))
            {
                return Unauthorized();
            }

            IEnumerable<TokenTransferHistory> flexTransactionHistory = null;
            try
            {
                flexTransactionHistory = _firewallFlexDataAccess.GetTransactionHistory(tokenId);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return Ok(flexTransactionHistory);
        }

        [HttpGet]
        public IHttpActionResult GetDeploymentProfileAudit(long userAccountId, long supportAccountId, string tokenId)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId) || !(HasFirewallFlexAccess(supportAccountId, userAccountId)))
            {
                return Unauthorized();
            }

            IEnumerable<DeploymentProfileAudit> deploymentProfileAudit = null;
            try
            {
                deploymentProfileAudit = _flexLogic.GetDeploymentProfileAudit(tokenId);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return Ok(deploymentProfileAudit);
        }

        [HttpGet]
        public IHttpActionResult GetDeploymentProfileAuditDetail(long userAccountId, long supportAccountId, long auditId)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId) || !(HasFirewallFlexAccess(supportAccountId, userAccountId)))
            {
                return Unauthorized();
            }

            IEnumerable<DeploymentProfileAuditDetail> deploymentProfileAuditDetail = null;
            try
            {
                deploymentProfileAuditDetail = _flexLogic.GetDeploymentProfileAuditDetail(auditId);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return Ok(deploymentProfileAuditDetail);
        }

        [HttpGet]
        public IHttpActionResult GetHyperVisors(long userAccountId, long supportAccountId, string firewallType = "")
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId) || !(HasFirewallFlexAccess(supportAccountId, userAccountId)))
            {
                return Unauthorized();
            }

            var hyperVisors = _firewallFlexDataAccess.GetHyperVisors(firewallType);
            return Ok(hyperVisors);
        }

        [HttpGet]
        public async Task<IHttpActionResult> GetPanOs(long userAccountId, long supportAccountId, string panOsVersion = "", 
            string firewallType = "")
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId) || !(HasFirewallFlexAccess(supportAccountId, userAccountId)))
            {
                return Unauthorized();
            }           
            var PanOsVersionsList = _networkSecurityApiLogic.GetPanOSVersions();
            PanOsVersions panOsVersions = JsonConvert.DeserializeObject<PanOsVersions>(JsonConvert.SerializeObject(PanOsVersionsList.ResponseObject));
            var response = _flexLogic.GetPanOsVersions(panOsVersion, panOsVersions);
            return Ok(response);
          
        }

        [HttpPost]
        public IHttpActionResult RegisterFirewall(FirewallRegisterRequest firewallRegisterRequest)
        {
            if (!JwtTokenManager.IsAuthorized(firewallRegisterRequest.UserAccountId, firewallRegisterRequest.SupportAccountId, ClaimType.UserAccountId)
                || !(HasFirewallFlexAccess(firewallRegisterRequest.SupportAccountId, firewallRegisterRequest.UserAccountId)))
            {
                return Unauthorized();
            }
            try
            {
                var response = _flexLogic.RegisterFireWall(firewallRegisterRequest);
                return Ok(response);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return Ok(new { success = false, message = Constants.RegisterFirewallFailedMessage });

        }

        [HttpPost]
        public IHttpActionResult PanoramaProvision(PanoramaProvisionModel model)
        {
            if (!JwtTokenManager.IsAuthorized(model.UserAccountId, model.SupportAccountId, ClaimType.UserAccountId)
                || !(HasFirewallFlexAccess(model.SupportAccountId, model.UserAccountId)))
            {
                return Unauthorized();
            }

            var status = false;
            var message = string.Empty;

            try
            {
                var response = _flexLogic.PanoramaProvision(model);
                message = (response != null) ? response.Message : "";
                status = response.Status;
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                message = Constants.PanoramaProvisionFailedMessage;
            }
            return this.Json(new { success = status, message = message });
        }

        [HttpPost]
        public IHttpActionResult DeletePanoramas(PanoramasDeleteRequest request)
        {
            if (!JwtTokenManager.IsAuthorized(request.UserAccountId, request.SupportAccountId, ClaimType.UserAccountId)
                || !(HasFirewallFlexAccess(request.SupportAccountId, request.UserAccountId)))
            {
                return Unauthorized();
            }

            var status = false;
            var message = string.Empty;
            try
            {
                return Json(_flexLogic.DeletePanoramas(request));
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                message = Constants.PanoramaDeleteFailedMessage;
            }
            return this.Json(new { success = status, message });
        }


        [HttpGet]
        public IHttpActionResult GetProvisionedPanoramas(long supportAccountId, long userAccountId, string authCode)
        {

            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId) || !(HasFirewallFlexAccess(supportAccountId, userAccountId)))
            {
                return Unauthorized();
            }

            try
            {
                return Json(_flexLogic.GetProvisionedPanoramas(authCode));
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new PanoramaProvisionedDevice[0]);
            }
        }


        [HttpGet]
        public IHttpActionResult DownloadLicense(long licenseId, long supportAccountId, long userAccountId)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId)
                || !(HasFirewallFlexAccess(supportAccountId, userAccountId)))
            {
                return Unauthorized();
            }

            try
            {
                var license = _flexLogic.DownloadLicenseKey(licenseId, userAccountId);
                var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                httpResponseMessage.Content = new ByteArrayContent(Encoding.ASCII.GetBytes(license.LicenseKey));
                httpResponseMessage.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                httpResponseMessage.Content.Headers.Add("Access-Control-Expose-Headers", "Content-Disposition,X-Suggested-Filename");
                httpResponseMessage.Content.Headers.ContentDisposition.FileName = license.LicenseName;
                return ResponseMessage(httpResponseMessage);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                var httpResponse = Request.CreateErrorResponse(HttpStatusCode.NotFound, "An unexpected error has occurred.");
                return ResponseMessage(httpResponse);
            }
        }


        [HttpGet]
        public IHttpActionResult DownloadPanoramaLicense(long supportAccountId, long userAccountId, long licenseId)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId) || !(HasFirewallFlexAccess(supportAccountId, userAccountId)))
            {
                return Unauthorized();
            }

            var response = _flexLogic.DownloadPanoramaLicense(licenseId, userAccountId);
            if (response != null && response.Success == true)
            {
                var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                httpResponseMessage.Content = new ByteArrayContent(Encoding.UTF8.GetBytes(response.LicenseKey));
                httpResponseMessage.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                httpResponseMessage.Content.Headers.Add("Access-Control-Expose-Headers", "Content-Disposition,X-Suggested-Filename");
                httpResponseMessage.Content.Headers.ContentDisposition.FileName = response.LicenseName;
                return ResponseMessage(httpResponseMessage);
            }
            else
            {
                var httpResponse = Request.CreateErrorResponse(HttpStatusCode.NotFound, "An unexpected error has occurred.");
                return ResponseMessage(httpResponse);
            }
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

        [HttpGet]
        public IHttpActionResult GetFirewalls(long supportAccountId, long userAccountId, string authCode, string type, string subType = null)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId)
               || !(HasFirewallFlexAccess(supportAccountId, userAccountId)))
            {
                return Unauthorized();
            }

            try
            {
                return Json(_flexLogic.GetFirewalls(supportAccountId, userAccountId, authCode, type, subType));
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new FlexDevice[0]);
            }
        }

        [HttpPost]
        public IHttpActionResult DeactivateFirewall(FirewallDeactivationRequest firewallDeactivationRequest)
        {
            if (!JwtTokenManager.IsAuthorized(firewallDeactivationRequest.UserAccountId, firewallDeactivationRequest.SupportAccountId, ClaimType.UserAccountId)
                || !(HasFirewallFlexAccess(firewallDeactivationRequest.SupportAccountId, firewallDeactivationRequest.UserAccountId)))
            {
                return Unauthorized();
            }

            long supportAccountId = _deviceLogic.GetSupportAccountIdByDeviceBySerialNumber(firewallDeactivationRequest?.SerialNumber[0]);
            if (firewallDeactivationRequest.SupportAccountId != supportAccountId)
            {
                return Unauthorized();
                var ex = new UnauthorizedAccessException("User does not have permission to  Deactivate the firewall");
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw ex;
            }

            var response = _flexLogic.DeactivateFirewall(firewallDeactivationRequest, User.Identity.Name);
            return Json(response);
        }

        [HttpGet]
        public IHttpActionResult GetPanoramasForMigration(long supportAccountId, long userAccountId, string authCode, string type, string subType = null)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId)
               || !(HasFirewallFlexAccess(supportAccountId, userAccountId)))
            {
                return Unauthorized();
            }

            try
            {
                return Json(_flexLogic.GetPanoramasForMigration(supportAccountId, authCode, type, subType));
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new FlexDevice[0]);
            }
        }

        [HttpGet]
        public IHttpActionResult GetPanoramaMigrationDetail(long supportAccountId, long userAccountId, long deviceId, string authCode)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId)
               || !(HasFirewallFlexAccess(supportAccountId, userAccountId)))
            {
                return Unauthorized();
            }

            try
            {
                return Json(_flexLogic.GetPanoramaMigrationDetail(deviceId, authCode));
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new FlexDevice[0]);
            }
        }

        [HttpPost]
        public IHttpActionResult RepurposePanoramas(RepurposePanoramasRequest panoramaTransferRequest)
        {
            if (!JwtTokenManager.IsAuthorized(panoramaTransferRequest.UserAccountId, panoramaTransferRequest.SupportAccountId, ClaimType.UserAccountId)
                || !(HasFirewallFlexAccess(panoramaTransferRequest.SupportAccountId, panoramaTransferRequest.UserAccountId)))
            {
                return Unauthorized();
            }

            try
            {
                var response = _flexLogic.MigratePanoramas(panoramaTransferRequest);
                return Json(response);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Error Processing Panorama Transfer"));
        }

        [HttpGet]
        public IHttpActionResult CreditPoolsForTransfer(long creditPoolId, long supportAccountId, long userAccountId)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId) || !HasFirewallFlexAccess(supportAccountId, userAccountId))
            {
                return Unauthorized();
            }
            try
            {
                return Json(_flexLogic.CreditPoolsForTransfer(creditPoolId, supportAccountId, userAccountId));
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Error in Getting Credit Pools for Deployment Profile transfer."));
        }

        [HttpPost]
        public IHttpActionResult TransferDeploymentProfile(TransferDeploymentProfilesRequest transferDeploymentProfilesRequest)
        {
            if (!JwtTokenManager.IsAuthorized(transferDeploymentProfilesRequest.UserAccountId, transferDeploymentProfilesRequest.SupportAccountId, ClaimType.UserAccountId)
                || !HasFirewallFlexAccess(transferDeploymentProfilesRequest.SupportAccountId, transferDeploymentProfilesRequest.UserAccountId))
            {
                return Unauthorized();
            }

            string message = string.Empty;
            try
            {
                message = ValidateDeploymentProfileGenType(transferDeploymentProfilesRequest);
                if (string.IsNullOrEmpty(message))
                {
                    transferDeploymentProfilesRequest.deploymentProfile.ProductId = transferDeploymentProfilesRequest.DestProductId;
                    transferDeploymentProfilesRequest.deploymentProfile.TokenId = transferDeploymentProfilesRequest.DestCreditPool;
                    transferDeploymentProfilesRequest.deploymentProfile.SupportType = transferDeploymentProfilesRequest.DestSupportType;
                    transferDeploymentProfilesRequest.deploymentProfile.Sku = transferDeploymentProfilesRequest.DestSku;
                    transferDeploymentProfilesRequest.deploymentProfile.ExpirationDate = transferDeploymentProfilesRequest.DestExpirationDate;
                    transferDeploymentProfilesRequest.deploymentProfile.AttributesValues = transferDeploymentProfilesRequest.deploymentProfile.AttributesValues; 

                    message = ValidateDeploymentProfileToTransfer(transferDeploymentProfilesRequest.deploymentProfile);
                    if (string.IsNullOrEmpty(message))
                    {
                        return Json(_flexLogic.TransferDeploymentProfiles(transferDeploymentProfilesRequest));
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                message = Constants.TransferDeploymentProfileFailureMessage;
            }
            return Json(new TransferDeploymentProfileResponse
            {
                Authcode = transferDeploymentProfilesRequest.deploymentProfile.AuthCode,
                Message = message,
                Success = false,

            });
        }

        [HttpGet]
        [JwtTokenAuthentication]
        public IHttpActionResult CloudCreditDetails(long userAccountId, long supportAccountId, long? creditPoolId = null, int page = 0, int size = 10)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId))
            {
                return Unauthorized();
            }
            try
            {
                return Json(_flexLogic.GetCloudCreditDetails(supportAccountId, creditPoolId, page, size));
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(string.Empty);
        }

        [HttpPost]
        [JwtTokenAuthentication]
        public IHttpActionResult FirewallRegistration(FirewallRegistrationRequest firewallRegistrationRequest)
        {
            if (!JwtTokenManager.IsAuthorized(firewallRegistrationRequest.UserAccountId, firewallRegistrationRequest.SupportAccountId, ClaimType.UserAccountId))
            {
                return Unauthorized();
            }
            try
            {
                var response = _flexLogic.FirewallRegistration(firewallRegistrationRequest);
                return this.Json(response);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(string.Empty);
        }

        [HttpGet]
        [JwtTokenAuthentication]
        public IHttpActionResult GetPanOSVersions(long userAccountId, long supportAccountId, string panOSVersion = "")
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId))
            {
                return Unauthorized();
            }
            try
            {
                var result = _flexLogic.GetPanOSVersions(userAccountId, supportAccountId, panOSVersion);
                return this.Json(result);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(string.Empty);
        }

        private double GetTotalTokens(DeploymentProfile deploymentProfile)
        {
            var subscriptionList = new List<string>();
            var subscriptions = "'" + string.Join("','", deploymentProfile.Subscriptions) + "'";

            if (deploymentProfile.Type == DeploymentProfileType.CN.ToDescription() && deploymentProfile.PanOS == "10.0")
            {
                subscriptionList = _flexLogic.GetSubscriptionsByUseCase(deploymentProfile.UseCaseId);
            }
            else
            {
                subscriptionList = new List<string>(deploymentProfile.Subscriptions);
            }

            var request = new TokenCalculatorRequest()
            {
                TokenId = deploymentProfile.TokenId,
                ProductId = deploymentProfile.ProductId,
                Panorama = deploymentProfile.Panorama,
                Type = deploymentProfile.Type,
                SubType = deploymentProfile.SubType, 
                QuantityFirewall = deploymentProfile.FirewallQuantity,
                NumberOfCPU = deploymentProfile.VcpuQuantity,
                Subscriptions = subscriptionList,
                KubernatesNodes = deploymentProfile.KubernatesNodes,
                PanOS = deploymentProfile.PanOS,
                SupportType = deploymentProfile.SupportType,
                NumberOfvSYS = deploymentProfile.VSysQuantity,
                AttributesValues = deploymentProfile.AttributesValues
            };

            var response = _flexLogic.GetEstimatedCost(request);
            return (response != null) ? response.TotalTokens : 0;
        }

        private string ValidateDeploymentProfile(DeploymentProfile deploymentProfile)
        {
            double estimatedTokens = 0;
            int firewallsCreated = 0;
            int allocationQuantity = 0;
            bool editProfile = false;
            bool isLegacy = false;

            if (!string.IsNullOrWhiteSpace(deploymentProfile.ProfileName) && _flexLogic.IsProfileNameExists(deploymentProfile.ProfileName, deploymentProfile.Id))
            {
                return Constants.ProfileNameExists;
            }
            else if (deploymentProfile.VcpuQuantity > Constants.VMSeriesVcpuQuantityMax && deploymentProfile.Type == DeploymentProfileType.VM.ToDescription())
            {
                return Constants.VMSeriesVcpuRestrictionMessage;
            }
            else
            {
                if (deploymentProfile.Id.HasValue && deploymentProfile.Id > 0)
                {
                    editProfile = true;
                    isLegacy = (deploymentProfile.Type == DeploymentProfileType.VM.ToDescription() && deploymentProfile.PanOS == ApiConstants.VMLegacyPanOs) || (deploymentProfile.Type == DeploymentProfileType.CN.ToDescription() && deploymentProfile.PanOS == ApiConstants.CNLegacyPanOs) ? true : false;
                    firewallsCreated = _firewallFlexDataAccess.GetFirewallsCreated(deploymentProfile.AuthCode);
                    allocationQuantity = isLegacy ? deploymentProfile.FirewallQuantity : (deploymentProfile.FirewallQuantity * deploymentProfile.VcpuQuantity);
                    if (allocationQuantity < firewallsCreated)
                    {
                        return Constants.CreditsUsed;
                    }
                }

                //get allocated and pending qty from csp and net balance from TMS
                var tokenAllocationDetail = _flexLogic.GetAllocationDetail(deploymentProfile.TokenId, deploymentProfile.ProductId, deploymentProfile.CreatedBy,
                              deploymentProfile.Id);


                // get estimated cost
                estimatedTokens = GetTotalTokens(deploymentProfile);

                if (deploymentProfile.isRenewal == false && estimatedTokens > tokenAllocationDetail.TokensAllocated
                    && Math.Floor((estimatedTokens - tokenAllocationDetail.TokensAllocated) * 100) / 100 > tokenAllocationDetail.NetBalance)
                {
                    return string.Format(Constants.NotEnoughCredits, tokenAllocationDetail.TotalAvailableTokens, tokenAllocationDetail.TotalAvailableTokens - tokenAllocationDetail.NetBalance, tokenAllocationDetail.NetBalance);
                }
                else if (deploymentProfile.isRenewal && estimatedTokens > tokenAllocationDetail.NetBalance)
                    
                {
                    return string.Format(Constants.NotEnoughCredits, tokenAllocationDetail.TotalAvailableTokens, tokenAllocationDetail.TotalAvailableTokens - tokenAllocationDetail.NetBalance, tokenAllocationDetail.NetBalance);
                }
                else
                {
                    deploymentProfile.TokensAllocated = estimatedTokens;
                }
            }
            return null;
        }


        private string ValidateDeploymentProfileToTransfer(DeploymentProfile deploymentProfile)
        {
            //get allocated and pending qty from csp and net balance from TMS
            var tokenAllocationDetail = _flexLogic.GetAllocationDetail(deploymentProfile.TokenId,
                                                                    deploymentProfile.ProductId,
                                                                    deploymentProfile.CreatedBy,
                                                                    null);
            // get estimated cost
            var estimatedTokens = GetTotalTokens(deploymentProfile);

            if (tokenAllocationDetail.NetBalance == 0 || (estimatedTokens > tokenAllocationDetail.TokensAllocated &&
                (estimatedTokens - tokenAllocationDetail.TokensAllocated) > tokenAllocationDetail.NetBalance))
            {
                return Constants.NotEnoughCreditsToTransfer;
            }



            deploymentProfile.TokensAllocated = estimatedTokens;

            return null;
        }

        private string GetExportToCsvData(FlexDevices devices, string type,bool addLastCheckinDate = false)
        {
            var csvData = new StringBuilder();
            if (type == DeploymentProfileType.VM.ToDescription())
            {
                if (addLastCheckinDate)
                {
                    csvData.Append("Serial Number,Vcpu,VM Model,License,Auth Code,Credit Qty Used,Expiration Date,Last CheckIn Date, Asc\n");

                    devices.FlexDeviceList.devices.ForEach(x =>
                    {
                        csvData.AppendFormat("{0},{1},{2},{3},{4},{5},{6},{7},{8}\n",
                            x.SerialNumber,
                            x.VCpuQuantity,
                            x.VMModel,
                           "\"" + string.Join("\n", x.Licenses.Select(y => y.Subscription)) + "\"",
                            x.AuthorizationCode,
                            x.CreditsUsed,
                            x.ExpirationDate,
                            x.LastCheckinDate,
                            x.AscName);
                    });

                }

                else
                {
                    csvData.Append("Serial Number,Vcpu,VM Model,License,Auth Code,Credit Qty Used,Expiration Date,Asc\n");

                    devices.FlexDeviceList.devices.ForEach(x =>
                    {
                        csvData.AppendFormat("{0},{1},{2},{3},{4},{5},{6},{7}\n",
                            x.SerialNumber,
                            x.VCpuQuantity,
                            x.VMModel,
                           "\"" + string.Join("\n", x.Licenses.Select(y => y.Subscription)) + "\"",
                            x.AuthorizationCode,
                            x.CreditsUsed,
                            x.ExpirationDate,
                            x.AscName);
                    });
                }
            }
            else if (type == DeploymentProfileType.CN.ToDescription())
            {
                csvData.Append("Serial Number,Panorama Model,Subscription Type,Auth Code,Credit Qty Used,Expiration Date,Asc\n");
                devices.FlexDeviceList.devices.ForEach(x =>
                {
                    csvData.AppendFormat("{0},{1},{2},{3},{4},{5},{6}\n",
                        x.SerialNumber,
                        x.DeviceName,
                       "\"" + string.Join("\n", x.Licenses.Select(y => y.Subscription)) + "\"",
                        x.AuthorizationCode,
                        x.CreditsUsed,
                        x.ExpirationDate,
                        x.AscName);
                });
            }
            return csvData.ToString();
        }


        private string ValidateDeploymentProfileGenType(TransferDeploymentProfilesRequest transferDeploymentProfilesRequest)
        {
            // Validations to transfer profile from Gen1 to Gen2 type Credit Pool

            var legacysubs = new List<string> { "WF","TP", "URL4" };
            
            if (transferDeploymentProfilesRequest.SourceCreditPoolGenType == CreditPoolGenType.Gen1.ToDescription() &&                
                transferDeploymentProfilesRequest.DestCreditPoolGenType == CreditPoolGenType.Gen2.ToDescription() && 
                transferDeploymentProfilesRequest.deploymentProfile.Subscriptions!=null && 
                transferDeploymentProfilesRequest.deploymentProfile.Subscriptions.Any(s=> legacysubs.Contains(s)))
            
            {
                return Constants.TransferDeploymentProfileIncompatibleMessage;
            }
            
            return null;
        }

    }
}
