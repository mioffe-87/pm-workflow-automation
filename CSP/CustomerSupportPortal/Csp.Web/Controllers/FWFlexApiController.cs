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
using Csp.Web.Attributes;

namespace Csp.Web.Controllers
{

    [RequireCspApiCert]
    public class FWFlexApiController : ApiController
    {
        private readonly IFirewallFlexDataAccess _firewallFlexDataAccess;
        private readonly IFirewallFlexLogic _flexLogic;
        private readonly ISecurityLogic _securityLogic;
        private readonly ISoftwareUpdateLogic _softwareUpdateLogic;
        private const int UseCaseId = 4; // Security Use Case id for Custom

        public FWFlexApiController(IFirewallFlexLogic flexLogic, IFirewallFlexDataAccess firewallFlexDataAccess, ISecurityLogic securityLogic, ISoftwareUpdateLogic softwareUpdateLogic)
        {
            _flexLogic = flexLogic;
            _firewallFlexDataAccess = firewallFlexDataAccess;
            _securityLogic = securityLogic;
            _softwareUpdateLogic = softwareUpdateLogic;
        }

        [HttpPost]
        public IHttpActionResult CreateDeploymentProfile(DeploymentProfile deploymentProfile)
        {
            string message = string.Empty;
            DeploymentProfileStatus deploymentProfileStatus = null;
            try
            {
                var responsePayload = _flexLogic.GetCreditDetails(deploymentProfile.SupportAccountId, deploymentProfile.UserAccountId);
                var creditPool = responsePayload.FirstOrDefault(x => x.TokenId == deploymentProfile.TokenId);

                deploymentProfile.ProductId = creditPool.ProductId;
                deploymentProfile.Sku = creditPool.Sku;
                deploymentProfile.ExpirationDate = DateTime.Parse(creditPool.ExpirationDate);
                deploymentProfile.SupportType = creditPool.SupportType;
                deploymentProfile.CreatedBy = deploymentProfile.UserAccountId;

                deploymentProfile.UseCaseId = new List<int> { UseCaseId };

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
                Id = 0,
                AuthCode = string.Empty,
                Success = false,
                Message = message
            };

            return Json(deploymentProfileStatus);
        }

        [HttpPost]
        public IHttpActionResult UpdateDeploymentProfile(DeploymentProfile deploymentProfile)
        {
            string message = string.Empty;
            DeploymentProfileStatus deploymentProfileStatus = null;
            try
            {
                var responsePayload = _flexLogic.GetCreditDetails(deploymentProfile.SupportAccountId, deploymentProfile.UserAccountId);
                var creditPool = responsePayload.FirstOrDefault(x => x.TokenId == deploymentProfile.TokenId);
                deploymentProfile.Id = _firewallFlexDataAccess.GetDeploymentProfile(deploymentProfile.AuthCode).Id;
                deploymentProfile.ProductId = creditPool.ProductId;
                deploymentProfile.Sku = creditPool.Sku;
                deploymentProfile.ExpirationDate = DateTime.Parse(creditPool.ExpirationDate);
                deploymentProfile.SupportType = creditPool.SupportType;
                deploymentProfile.CreatedBy = deploymentProfile.UserAccountId;

                deploymentProfile.UseCaseId = new List<int> { UseCaseId };

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
                Id = 0,
                AuthCode = string.Empty,
                Success = false,
                Message = message
            };

            return Json(deploymentProfileStatus);
        }

        [HttpDelete]
        public IHttpActionResult DeleteDeploymentProfile(DeleteDeploymentProfileRequest req)
        {
            DeploymentDeleteResponse response = null;
            try
            {
                if (!_firewallFlexDataAccess.IsUserHasPermissionToDelete(req.AuthCode, req.UserAccountId, req.SupportAccountId) || req.SupportAccountId != req.SupportAccountId)
                {
                    return Unauthorized();
                }

                var deploymentProfile = _firewallFlexDataAccess.GetDeploymentProfile(0, req.AuthCode);
                var responsePayload = _flexLogic.GetCreditDetails(req.SupportAccountId, req.UserAccountId);
                var creditPool = responsePayload.FirstOrDefault(x => x.TokenId == deploymentProfile.TokenId);

                req.ProductId = creditPool.ProductId;
                req.Type = deploymentProfile.Type;
                req.SubType = deploymentProfile.SubType;
                req.PanOS = deploymentProfile.PanOS;
                req.TokenId = deploymentProfile.TokenId;

                var result = _flexLogic.DeleteDeploymentProfile(req.UserAccountId, req.AuthCode, req.ProductId, req.TokenId, req.Type, req.PanOS, req.SubType);
                response = new DeploymentDeleteResponse
                {
                    Authcode = result.Authcode,
                    Success = result.Success,
                    Message = result.Success && string.IsNullOrEmpty(result.Message) ? "Deleted" : result.Message,
                };

            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, response));
            }
            return Json(response);
        }

        [HttpGet]
        public IHttpActionResult GetCreditPoolDetails(long userAccountId, long supportAccountId)
        {
            try
            {
                var response = _flexLogic.GetCreditDetails(supportAccountId, userAccountId);

                var tokenInfo = new TokenDto()
                {
                    data = Mapper.Map<IList<TokenInfo>>(response)
                };
                return Ok(tokenInfo);

            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new TokenDto[0]);
            }
        }


        [HttpGet]
        public IHttpActionResult GetCreditPoolDetailsByTokenId(long userAccountId, long supportAccountId, long tokenId)
        {
            try
            {
                TokenInfo tokenInfo = null;
                var response = _flexLogic.GetCreditDetails(supportAccountId, userAccountId);

                var tokenListInfo = new TokenDto()
                {
                    data = Mapper.Map<IList<TokenInfo>>(response)
                };

                if (tokenListInfo != null && tokenListInfo.data != null)
                {
                    tokenInfo = tokenListInfo.data.FirstOrDefault(x => x.TokenId == tokenId);
                }

                return Ok(new { data = tokenInfo });

            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new TokenDto[0]);
            }
        }

        [HttpGet]
        public IHttpActionResult GetDeploymentProfiles(long userAccountId, long supportAccountId, long tokenId)
        {
            try
            {
                var response = _flexLogic.GetDeploymentProfiles(userAccountId, supportAccountId, tokenId, DateTime.MinValue.ToShortDateString());
                var deploymentProfiles = new DeploymentProfiles()
                {
                    data = Mapper.Map<IList<DeploymentProfileData>>(response.profiles)
                };
                return Ok(deploymentProfiles);

            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new DeploymentProfiles[0]);
            }
        }

        [HttpGet]
        public IHttpActionResult GetDeploymentProfile(long userAccountId, long supportAccountId, string authCode)
        {
            try
            {
                var deploymentProfile = _flexLogic.GetDeploymentProfile(authCode, userAccountId, supportAccountId);

                var deploymentProfiles = new DeploymentProfileDto()
                {
                    data = Mapper.Map<DeploymentProfileData>(deploymentProfile)
                };
                return Ok(deploymentProfiles);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new DeploymentProfile[0]);
            }
        }

        [HttpGet]
        public IHttpActionResult GetDeploymentProfiles(long supportAccountId)
        {
            try
            {
                var response = new DeploymentProfileHubDto
                {
                    data = _flexLogic.GetDeploymentProfiles(supportAccountId)
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new DeploymentProfileHubDto[0]);
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
                QuantityFirewall = deploymentProfile.FirewallQuantity,
                NumberOfCPU = deploymentProfile.VcpuQuantity,
                Subscriptions = subscriptionList,
                KubernatesNodes = deploymentProfile.KubernatesNodes,
                PanOS = deploymentProfile.PanOS,
                SupportType = deploymentProfile.SupportType,
                NumberOfvSYS = deploymentProfile.VSysQuantity
            };

            var response = _flexLogic.GetEstimatedCost(request);
            return (response != null) ? response.TotalTokens : 0;
        }

        private string ValidateDeploymentProfile(DeploymentProfile deploymentProfile)
        {
            double estimatedTokens = 0;
            int firewallsCreated = 0;
            int allocationQuantity = 0;
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

                if (estimatedTokens > tokenAllocationDetail.TokensAllocated
                    && (estimatedTokens - tokenAllocationDetail.TokensAllocated) > tokenAllocationDetail.NetBalance)
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
    }
}
