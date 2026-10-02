using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.Services.Contracts;
using Csp.Api.Model;
using Csp.Api.Model.Flex;
using Csp.Api.HelperClasses;
using System.Net.Http;
using System.Configuration;
using System.Web.Script.Serialization;
using System.IO;
using Newtonsoft.Json;
using Csp.Api.Model.Enums;
using AutoMapper;
using System.Security.Cryptography;
using Csp.Api.LicensingService;
using Csp.Api.DataAccess;
using Csp.Api.Security;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Net;

namespace Csp.Api.Logic
{
    public class FirewallFlexLogic : IFirewallFlexLogic
    {
        #region Constants
        private readonly ILicenseLogic _licenseLogic;
        private readonly IFirewallFlexDataAccess _flexDataAccess;
        private readonly ISecurityLogic _securityLogic;
        private readonly IApplicationSettings _applicationSettings;

        private readonly string _tokenServiceThumbprint = ConfigurationManager.AppSettings["SfdcApiThumbprint"];

        private readonly string _licensingApi = ConfigurationManager.AppSettings["SfdcApi"];
        private readonly string _licensingUserName = ConfigurationManager.AppSettings["SfdcApiUserName"];
        private readonly string _licensingPassword = ConfigurationManager.AppSettings["SfdcApiPassword"];

        private readonly string _usageApi = ConfigurationManager.AppSettings["UsageApi"];
        private readonly string _usageApiUserName = ConfigurationManager.AppSettings["UsageApiUserName"];
        private readonly string _usageApiPassword = ConfigurationManager.AppSettings["UsageApiPassword"];

        private readonly string _tokenCalculatorApi = ConfigurationManager.AppSettings["TokenCalculatorApi"];
        private readonly string _tokenCalculatorApiUserName = ConfigurationManager.AppSettings["TokenCalculatorApiUserName"];
        private readonly string _tokenCalculatorApiPassword = ConfigurationManager.AppSettings["TokenCalculatorApiPassword"];

        private readonly string _projectId = ConfigurationManager.AppSettings.Get("MicrosvcsProjectId");
        private readonly string _topic = ConfigurationManager.AppSettings.Get("MicrosvcsTopic");

        private readonly string provisioningPubSubProjectId = ConfigurationManager.AppSettings.Get("ProvisioningPubSubProjectId");
        private readonly string provisioningPubSubTopic = ConfigurationManager.AppSettings.Get("ProvisioningPubSubTopic");

        private readonly string _aesKey256 = ConfigurationManager.AppSettings.Get("AesKey256");

        private readonly string _retryUrlService = "NovaTokenService";

        private readonly Dictionary<string, string> supportTypeMapping = new Dictionary<string, string>()
        {
            {"Premium","PREM"},
            {"Platinum","PLAT"},
            {"Premium Partner","BKLN"},
            {"US Government Premium","PREMUSG"},
            {"US Government Premium Partner","BKLNUSG"},
            {"Standard","STND"},
            {"US Government Standard","STNDUSG"},
            {"Premium with Automation Support","PREMAUT" },
            {"Platinum with Automation Support","PLATAUT" },
            {"Partner Enabled Premium Support","BKLN"}
        };

        private string DEVICE_STATE_ACTIVE = "Active";
        private string DEVICE_STATE_WARM = "Warm";

        public const string FLEX_ATTRIBUTE_NAME_BT = "billion_tokens";
        public const string FLEX_ATTRIBUTE_NAME_RT = "redTeaming";
        public const string FLEX_ATTRIBUTE_NAME_MS = "modelSecurity";
        public const string FLEX_ATTRIBUTE_NAME_GW_DEPLOYMENT = "deployment";
        public const string FLEX_ATTRIBUTE_NAME_GW_GW_GATEWAY = "gateway";
        public const string FLEX_ATTRIBUTE_NAME_GW_REQUESTS = "billion_tokens";
        public const string FLEX_ATTRIBUTE_NAME_APIM = "credits";
        public const string FLEX_ATTRIBUTE_NAME_AES = "numberOfEp";
        public const string FLEX_ATTRIBUTE_NAME_CLOUD_CNGFW = "credits";
        public const string FLEX_ATTRIBUTE_NAME_SPM = "numberOfWorkloads";

        #endregion

        public FirewallFlexLogic(IFirewallFlexDataAccess flexDataAccess, ILicenseLogic licenseLogic, ISecurityLogic securityLogic, IApplicationSettings applicationSettings)
        {
            _licenseLogic = licenseLogic;
            _flexDataAccess = flexDataAccess;
            _securityLogic = securityLogic;
            _applicationSettings = applicationSettings;
        }

        public IEnumerable<SupportAccountLicense> GetSupportAccountsLicenses(long userAccountId)
        {
            return _flexDataAccess.GetSupportAccountsLicenses(userAccountId);
        }

        public IEnumerable<CreditDataModel> GetCreditDetails(string tokenIds, long userAccountId)
        {
            if (string.IsNullOrEmpty(tokenIds))
            {
                return Enumerable.Empty<CreditDataModel>();
            }
            var decryptedTokenIds = Decrypt(tokenIds);

            return GetCreditDataModel(decryptedTokenIds, userAccountId).Where(c => c.SupportAccountId == null || c.SupportAccountId <= 0);
        }

        public IList<CreditDataModel> GetCreditDetails(long supportAccountId, long userAccountId)
        {
            var credits = new List<CreditDataModel>();
            var parentPoolcredits = new List<CreditDataModel>();
            var profileInfoList = new List<DeploymentProfileInfo>();

            UserAccountDataAccess userAccountDataAccess = new UserAccountDataAccess();

            var userAccount = userAccountDataAccess.Read(userAccountId, ApiConstants.PANW_SYSTEM_USER_ACCOUNT_ID);

            if (userAccount == null)
            {
                return new List<CreditDataModel>();
            }
            CspSession userSession = _securityLogic.GetSessionDetails(userAccount.Email, supportAccountId);

            if (userSession == null)
            {
                return new List<CreditDataModel>();
            }

            bool hasAccess = userSession.SecurityClassifications.Any(p => p.SupportAccountId == supportAccountId) ||
             userSession.SecurityClassifications.Any(x => x.SecurityClassification == (int)SecurityClassification.Global);

            List<string> ascAccountIds = new List<string>();
            if (!hasAccess)
            {
                ascAccountIds = userAccountDataAccess.GetAscSalesForceAccountIdsByUserAccountId(userAccountId);
            }

            var responsePayload = GetFlexData(userAccountId, ApiConstants.SUPPORTACCOUNTID_TYPE, ApiConstants.SUPPORTACCOUNTID_VARIABLE,
                    ApiConstants.SUPPORTACCOUNTID_VALUE, supportAccountId.ToString(), supportAccountId);

            if (!hasAccess && ascAccountIds.Count > 0)
            {
                List<CreditDetail> filteredData = responsePayload.Credits.FindAll(x => ascAccountIds.Contains(x.AscAccountId));
                responsePayload.Credits = filteredData;
            }
            if (!hasAccess && ascAccountIds.Count == 0)
            {
                return new List<CreditDataModel>();
            }

            if (responsePayload != null && responsePayload.Credits != null)
            {
                profileInfoList = _flexDataAccess.GetNumberofDeployments(supportAccountId).ToList();
                Mapper.Map(responsePayload.Credits, credits);
            }

            // Get renewal deployment profiles info for each credit pool
            var renewalProfileInfoList = new List<DeploymentProfileInfo>();
            foreach (var credit in credits)
            {
                var renewalProfiles = _flexDataAccess.GetNumberofRenewalDeployments((long)credit.SupportAccountId, credit.TokenId).ToList();
                renewalProfileInfoList.AddRange(renewalProfiles);
            }

            foreach (var credit in credits)
            {
                var profileInfo = profileInfoList.FirstOrDefault(x => x.SupportAccountId == credit.SupportAccountId && x.TokenId == credit.TokenId);
                if (profileInfo != null)
                {
                    credit.NumberOfDeployments = profileInfo.NumberOfDeployments;
                }
                
                // Check if there are renewal deployment profiles and set IsGracePeriodDp to true
                var renewalProfileInfo = renewalProfileInfoList.FirstOrDefault(x => x.SupportAccountId == credit.SupportAccountId && x.TokenId == credit.TokenId);
                if (renewalProfileInfo != null && renewalProfileInfo.NumberOfDeployments > 0)
                {
                    credit.IsGracePeriodDp =  true;
                }
                
                credit.IsExpired = (credit.Expiration.HasValue && credit.Expiration < DateTime.Now);
                credit.IsRenewalConfirmed = string.Compare(credit.Status, CreditPoolStatus.Active.ToString(), StringComparison.OrdinalIgnoreCase) == 0 ?
                                            IsRenewalConfirmed(responsePayload, credit.TokenId) : false;

                if (string.Compare(credit.TransactionType, "Renew", StringComparison.OrdinalIgnoreCase) != 0)
                {
                    if (Convert.ToInt64(credit.ParentTokenId) != 0)
                    {
                        var parentPoolResponse = GetFlexData(userAccountId, ApiConstants.TOKENID_TYPE, ApiConstants.TOKENID_VARIABLE, ApiConstants.TOKENID_VALUE, credit.ParentTokenId.ToString());

                        if (parentPoolResponse != null && parentPoolResponse.Credits != null)
                        {
                            Mapper.Map(parentPoolResponse.Credits, parentPoolcredits);
                        }
                        var creditsListForToken = parentPoolcredits.ToList();
                        credit.PoolMessages = GetCreditPoolMessages(creditsListForToken);
                        //credit.ExpirationDate = GetAmendRampupExpirationDate(creditsListForToken);
                    }

                    else
                    {
                        var creditsListForToken = credits.Where(x => x.TokenId == credit.TokenId).ToList();
                        credit.PoolMessages = GetCreditPoolMessages(creditsListForToken);
                        //credit.ExpirationDate = GetAmendRampupExpirationDate(creditsListForToken);
                    }
                }
            }

            // Get FedRAMP Type
            EFedRampType fedRampType = _flexDataAccess.GetFedRampType(supportAccountId);

            credits.ForEach(i => i.AccountFedRampType = fedRampType);

            var activeCreditPools = credits.Where(x => string.Compare(x.Status, CreditPoolStatus.Active.ToString(), StringComparison.OrdinalIgnoreCase) == 0).ToList();
            var expiredCreditPools = credits.Where(x => string.Compare(x.Status, CreditPoolStatus.Expired.ToString(), StringComparison.OrdinalIgnoreCase) == 0).ToList();

            if (expiredCreditPools.Count > 0)
            {
                var activeTokenIds = activeCreditPools.Select(x => x.TokenId).ToList();
                activeCreditPools = activeCreditPools.Concat(expiredCreditPools.Where(x => !activeTokenIds.Contains(x.TokenId))).ToList();
            }

            IEnumerable<(long tokenId, bool hasSaasCloud, bool HasCloudNgfw)> creditPoolTraits = _flexDataAccess.GetCreditPoolTraits(activeCreditPools.Select(c => c.TokenId).ToArray());
            creditPoolTraits.Where(t => t.hasSaasCloud || t.HasCloudNgfw).ToList().ForEach(t =>
            {
                CreditDataModel creditModel = activeCreditPools.FirstOrDefault(c => t.tokenId == c.TokenId);
                if (creditModel != null)
                {
                    creditModel.Traits = new CreditDataTraitsModel
                    {
                        HasCloudNgfw = t.HasCloudNgfw,
                        HasSaasCloud = t.hasSaasCloud
                    };
                }
            });

            return activeCreditPools;
        }

        public bool DepositCredits(List<DepositCreditRequest> request, long supportAccountId, long userAccountId)
        {
            var requestDetail = GetRequestDetail(FwFlexRequestType.TokenActivate.ToDescription(), request);

            CheckandAssignCreditAdminRole(supportAccountId, userAccountId);

            var creditResponseList = TokenHelper.PostAsync<TokenResponse>(_usageApi, requestDetail.RequestUrl, requestDetail.Credential, _tokenServiceThumbprint,
                                    requestDetail.JsonRequest, FwFlexRequestType.TokenActivate, supportAccountId, userAccountId);
            request.ForEach(x =>
            {
                var creditTransferRequest = new CreditTransferRequest
                {
                    SourceSupportAccountId = x.SupportAccountId,
                    DestinationSupportAccountId = x.SupportAccountId,
                    DestinationTokenId = x.TokenId,
                    Quantity = x.Quantity,
                    UserAccountId = userAccountId
                };
                _flexDataAccess.LogCreditTransfer(creditTransferRequest);
            });

            return creditResponseList != null && creditResponseList.Count() > 0 ? true : false;
        }

        /// <summary>
        /// Post to Pubsub.
        /// </summary>
        /// <param name="model">Model from UI.</param>
        /// <returns>Success or failure</returns>
        public bool DepositCredits(DepositCreditModel model)
        {
            string url = string.Format("{0}/{1}", provisioningPubSubProjectId, provisioningPubSubTopic);
            try
            {
                string tokenIds = string.Join(",", model.CreditAssociations?.Select(i => i.TokenId.ToString()).ToArray() 
                    ?? new string[0]);
                if (!string.IsNullOrWhiteSpace(tokenIds))
                {
                    // Get token details
                    string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(_licensingUserName + ":" + _licensingPassword));
                    string graphQLRequest = string.Format(ApiConstants.TOKEN_SUMMARY_FROM_ENTITLEMENT_GRAPHQL, tokenIds, "ACTIVE", ApiConstants.CLOUD_NGFW_PRODUCT_ID);
                    CreditDataFromEntitlement flexData = TokenHelper.GetFlexData<CreditDataFromEntitlement>(_licensingApi, credentials, "POST", _tokenServiceThumbprint, graphQLRequest, model.UserAccountId, model.SupportAccountId);
                    CreditDetailFromEntitlement firstItem = flexData?.Credits?.FirstOrDefault();
                    CreditDetailFromEntitlementItemEntitlement firstItemEntitlement = firstItem?.Entitlements?.FirstOrDefault();
                    if (firstItem != null
                        && firstItemEntitlement != null)
                    {
                        // Compose Pubsub request payload
                        dynamic data = new
                        {
                            entitlement_group_id = firstItem.EntitlementGroupId,
                            entitlement_id = firstItemEntitlement.EntitlementId,
                            instance_identifiers = firstItem.Entitlements.Where(i => i.Status == "ACTIVE").Select(i => new
                            {
                                serial_number = i.TokenId.ToString(),
                                token_id = i.TokenId.ToString(),
                                active = true
                            }),
                            entitlement_event = "BUY",
                            event_type = "PROVISIONING_COMPLETED",
                            instance_url = firstItemEntitlement.InstanceUrl,
                            support_account_id = model.SupportAccountId,
                            processed_date = firstItemEntitlement.OrderDate,
                            processed_by_system = "AWS",
                            region = firstItemEntitlement.Configuration?.FirstOrDefault()?.Keys?.Contains("region") ?? false
                                ? firstItemEntitlement.Configuration.FirstOrDefault()["region"]
                                : string.Empty
                        };
                        string serializedData = JsonConvert.SerializeObject(data);
                        try
                        {
                            EventPublisher eventPublisher = new EventPublisher();
                            // Publish to Pubsub
                            Task<string> pubsubTask = Task.Run<string>(async () => await eventPublisher.PublishCustomEventNoExceptionHandling(data, provisioningPubSubProjectId, provisioningPubSubTopic));
                            pubsubTask.Wait();
                            string messageId = pubsubTask.Result;
                            _flexDataAccess.LogAuditHistory(model.SupportAccountId, FwFlexRequestType.TokenActivate, url, 
                                serializedData, messageId, model.UserAccountId);
                            return true;
                        }
                        catch (Exception exPubsub)
                        {
                            _flexDataAccess.LogAuditHistory(model.SupportAccountId, FwFlexRequestType.TokenActivate, url, 
                                serializedData, string.Format("Pubsub call exception: '{0}'; Inner exception: {1}.", exPubsub.Message, exPubsub.InnerException?.Message), model.UserAccountId);
                        }
                    }
                }
                else
                {
                    // log empty tokenIds
                    _flexDataAccess.LogAuditHistory(model.SupportAccountId, FwFlexRequestType.TokenActivate, url, 
                        string.Format("FirewallFlexLogic::DepositCredits({0}) returned no valid data.", JsonConvert.SerializeObject(model)),
                        string.Empty, model.UserAccountId);
                }
            }
            catch (Exception ex)
            {
                // Log in LDB
                _flexDataAccess.LogAuditHistory(model.SupportAccountId, FwFlexRequestType.TokenActivate, url,
                    string.Format("FirewallFlexLogic::DepositCredits({0}) threw exception: '{1}'.", JsonConvert.SerializeObject(model), ex),
                    string.Empty, model.UserAccountId);
                // Log in Elmah
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return false;
        }

        public bool EditPoolName(PoolEditReq request, long userAccountId)
        {
            request.CrossRefTransactionId = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            request.TransactionDate = DateTime.UtcNow;

            var requestToTMS = new List<PoolEditReq>() { request };


            var requestDetail = GetRequestDetail(FwFlexRequestType.RenamePool.ToDescription(), requestToTMS);
 
            var creditResponseList = TokenHelper.PostAsync<PoolEditReq>(_usageApi, requestDetail.RequestUrl, 
                                                                        requestDetail.Credential,
                                                                        _tokenServiceThumbprint,
                                                                        requestDetail.JsonRequest, 
                                                                        FwFlexRequestType.RenamePool,
                                                                        request.SupportAccountId, 
                                                                        userAccountId);
           
            return creditResponseList != null && creditResponseList.Count() > 0 ? true : false;
        }


        public bool CreditTransfer(CreditTransferRequest request)
        {
            if (request != null && request.DestinationTokenId == 0)
            {
                request.DestinationTokenId = null;
            }

            if (request != null & request.SourceTokenId == request.DestinationTokenId)
                throw new Exception("SourceTokenId and DestinationTokenId should not be same.");

            var requestDetail = GetRequestDetail(FwFlexRequestType.TokenTransfer.ToDescription(), request);

            var creditTransferResponse = TokenHelper.PostAsync<TokenResponse>(_usageApi, requestDetail.RequestUrl, requestDetail.Credential, _tokenServiceThumbprint,
                                   "[" + requestDetail.JsonRequest + "]", FwFlexRequestType.TokenTransfer, request.SourceSupportAccountId, request.UserAccountId);

            if (creditTransferResponse != null && creditTransferResponse.Any())
            {
                if (request.DestinationTokenId == null)
                {
                    var response = creditTransferResponse.Where(c => c.TokenId == request.SourceTokenId).FirstOrDefault();
                    request.DestinationTokenId = response != null && response.NewTokenPool ? response.TargetTokenId : request.DestinationTokenId;
                }
                _flexDataAccess.LogCreditTransfer(request);
                return true;
            }
            return false;
        }

        public IEnumerable<DeploymentProfile> GetExpiredDeploymentProfiles(long supportAccountId, long userAccountId, long tokenId)
        {
            var expiredProfiles = _flexDataAccess.GetExpiredDeploymentProfiles(supportAccountId, tokenId).ToList();

            string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(_licensingUserName + ":" + _licensingPassword));
            var graphQLRequest = string.Format(ApiConstants.TOKEN_SUMMARY_QUANTITY_ALLOCATION, tokenId);
            var responsePayload = TokenHelper.GetFlexData<CreditDetail>(_licensingApi, credentials, "POST", _tokenServiceThumbprint, graphQLRequest, userAccountId, supportAccountId);

            if (responsePayload == null || responsePayload.QuantityAllocation == null) return expiredProfiles;

            foreach (var profile in expiredProfiles)
            {
                var quantityAllocation = responsePayload.QuantityAllocation.Where(y => y.AllocationId == profile.AuthCode);
                if (quantityAllocation != null)
                {
                    profile.TokensAllocated = quantityAllocation.Sum(y => y.QuantityAllocated);
                    profile.TokensConsumed = quantityAllocation.Sum(y => y.QuantityConsumed);
                }
            }

            return expiredProfiles;
        }

        public CreditDetail GetTokensAllocatedAndConsumed(long supportAccountId, long userAccountId, long tokenId)
        {
            string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(_licensingUserName + ":" + _licensingPassword));
            var graphQLRequest = string.Format(ApiConstants.TOKEN_SUMMARY_QUANTITY_ALLOCATION, tokenId);
            return TokenHelper.GetFlexData<CreditDetail>(_licensingApi, credentials, "POST", _tokenServiceThumbprint, graphQLRequest, userAccountId, supportAccountId);
        }

        public TokenCalculatorResponse GetEstimatedCost(Csp.Api.Model.TokenCalculatorRequest request)
        {
            if (request.Panorama != null && request.Panorama.Count > 0)
            {
                request.Panorama = request.Panorama.ConvertAll(delegate (string x)
                {
                    return ((PanoramaType)Enum.Parse(typeof(PanoramaType), x)).ToDescription();
                });
            }
            request.KubernatesNodes = request.Type == DeploymentProfileType.CN.ToDescription() && request.PanOS == ApiConstants.CNLegacyPanOs ? request.QuantityFirewall : request.KubernatesNodes;
            request.QuantityFirewall = GetFirewallQuantity(request);
            request.NumberOfCPU = request.Type == DeploymentProfileType.CN.ToDescription() && request.PanOS == ApiConstants.CNLegacyPanOs ? 0 : request.NumberOfCPU;
            request.NumberOfvSYS = request.Type == DeploymentProfileType.VM.ToDescription() ? request.NumberOfvSYS : 0;

            request.PanOS = null;
            var jsonRequest = JsonConvert.SerializeObject(request);
            string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(_tokenCalculatorApiUserName + ":" + _tokenCalculatorApiPassword));
            var tokenCalculatorServiceResponse = TokenHelper.GetFlexData<TokenCalculatorResponse>(_tokenCalculatorApi, credentials, "POST", _tokenServiceThumbprint, jsonRequest);
            return tokenCalculatorServiceResponse;
        }

        public DeploymentProfileStatus AddUpdateDeploymentProfile(DeploymentProfile deploymentProfile)
        {
            DeploymentProfileStatus deploymentProfileStatus = null;
           
            var deploymentProfileResponse = new DeploymentProfileResponse();
            var features = _flexDataAccess.GetFlexFeatures(Convert.ToInt32(deploymentProfile.Id));
            if (features != null)            {
                deploymentProfile.Subscriptions = deploymentProfile.Subscriptions.Except<string>(features).ToArray<string>();               
            }
            deploymentProfile.Subscriptions = deploymentProfile.Subscriptions.Union(deploymentProfile?.Features).ToArray<string>();

            var subscriptions = "'" + string.Join("','", deploymentProfile.Subscriptions) + "'";
            // add vsys quantity 
            if (deploymentProfile.Type == DeploymentProfileType.VM.ToDescription() && deploymentProfile.VSysQuantity >=1)
            {
                string vsysasSubscriptions = "VSYS-" + deploymentProfile.VSysQuantity.ToString();
                subscriptions = subscriptions +"," + "'" + string.Join("','", vsysasSubscriptions) + "'";
            }

            string supportType = supportTypeMapping.FirstOrDefault(x => x.Key == deploymentProfile.SupportType).Value;
                var ascAccountInfo = new AscAccountInfo();
            if (supportType == "BKLN" || supportType == "BKLNUSG")
            {
                var creditDetail = GetFlexData(deploymentProfile.CreatedBy, ApiConstants.TOKENID_TYPE, ApiConstants.TOKENID_VARIABLE, ApiConstants.TOKENID_VALUE,
                    deploymentProfile.TokenId.ToString());

                if (creditDetail != null && creditDetail.Credits.Any())
                {
                    var activeCreditPools = creditDetail.Credits.Where(x => string.Compare(x.Status, CreditPoolStatus.Active.ToString(), StringComparison.OrdinalIgnoreCase) == 0).ToList();
                    ascAccountInfo = _flexDataAccess.GetAscAccountInfo(activeCreditPools.FirstOrDefault().AscAccountId);
                }
            }

            var creditData = GetFlexData(deploymentProfile.CreatedBy, ApiConstants.SUPPORTACCOUNTID_TYPE, ApiConstants.SUPPORTACCOUNTID_VARIABLE,
                            ApiConstants.SUPPORTACCOUNTID_VALUE, deploymentProfile.SupportAccountId.ToString(), deploymentProfile.SupportAccountId);
            string flattenedSubtype = _flexDataAccess.FlattenSubType(deploymentProfile.Type, deploymentProfile.SubType);
            // NOTE:In Java, keep as close as possible to the logic in Java.
            // Ideally, 'flattenedSubtype' is the only change in the 'var deploymentProfileRequest = new DeploymentProfileRequest()'
            var deploymentProfileRequest = new DeploymentProfileRequest()
            {
                Authcode = deploymentProfile.AuthCode,
                SupportAccountId = (Int32)deploymentProfile.SupportAccountId,
                VcpuQuantity = deploymentProfile.Type == DeploymentProfileType.CN.ToDescription() ? deploymentProfile.VcpuQuantity : (deploymentProfile.VcpuQuantity * deploymentProfile.FirewallQuantity),
                Subscriptions = subscriptions,
                ExpirationDate = IsRenewalConfirmed(creditData, deploymentProfile.TokenId) ? deploymentProfile.ExpirationDate.AddDays(ApiConstants.RenewalGracePeriod) : deploymentProfile.ExpirationDate,
                TokenId = deploymentProfile.TokenId.ToString(),
                Entitlements = subscriptions,
                IsLegacy = (deploymentProfile.Type == DeploymentProfileType.VM.ToDescription() && deploymentProfile.PanOS == ApiConstants.VMLegacyPanOs) || (deploymentProfile.Type == DeploymentProfileType.CN.ToDescription() && deploymentProfile.PanOS == ApiConstants.CNLegacyPanOs) ? true : false,
                NumberOfFirewalls = deploymentProfile.FirewallQuantity,
                SupportType = supportTypeMapping.FirstOrDefault(x => x.Key == deploymentProfile.SupportType).Value,
                PartId = deploymentProfile.Type == DeploymentProfileType.CN.ToDescription() && deploymentProfile.PanOS == ApiConstants.CNLegacyPanOs ? string.Format("PAN-CN-X-{0}-{1}", deploymentProfile.Subscriptions[0], supportTypeMapping.FirstOrDefault(x => x.Key == deploymentProfile.SupportType).Value) : deploymentProfile.Sku,
                // NOTE: 'flattenedSubtype' is the required change
                VmType = string.IsNullOrWhiteSpace(flattenedSubtype)
                    ? (deploymentProfile.Type == DeploymentProfileType.VM.ToDescription() && deploymentProfile.PanOS == ApiConstants.VMLegacyPanOs) ? deploymentProfile.VmModel : deploymentProfile.Type
                    : flattenedSubtype,
                AscId = ascAccountInfo.AscId,
                AscName = ascAccountInfo.AscName,
                IsAirgapEligible = deploymentProfile.IsAirgapEligible,
                Tokens = new CnTokens
                {
                    core_tokens = deploymentProfile.VcpuQuantity,
                    node_tokens = deploymentProfile.KubernatesNodes
                },
                IsSubscriptionOnly = deploymentProfile.ProductId == ApiConstants.Subscription
            };

            var isCreateDP = string.IsNullOrEmpty(deploymentProfile.AuthCode);

            if (deploymentProfile.Type == DeploymentProfileType.CN.ToDescription() && ApiConstants.CNNonlegacyPanOs.Any(s => s.Contains(deploymentProfile.PanOS)))
            {
                var cnxDeploymentProfileRequest = new CnxDeploymentProfileRequest
                {
                    DpAuthcode = deploymentProfileRequest.Authcode,
                    Entitlements = string.Join(",", deploymentProfile.Subscriptions),
                    Expiration = deploymentProfileRequest.ExpirationDate,
                    PartId = deploymentProfileRequest.PartId,
                    SupportAccountId = deploymentProfileRequest.SupportAccountId,
                    TokenId = deploymentProfileRequest.TokenId,
                    Tokens = JsonConvert.SerializeObject(deploymentProfileRequest.Tokens),
                    SupportType = supportTypeMapping[deploymentProfile.SupportType],
                    AscId = ascAccountInfo.AscId,
                    AscName = ascAccountInfo.AscName
                };

                deploymentProfileResponse = isCreateDP
                    ? _licenseLogic.GetCnxDeploymentProfile(cnxDeploymentProfileRequest)
                    : _licenseLogic.EditCnxDeploymentProfile(cnxDeploymentProfileRequest);

                _flexDataAccess.LogAuditHistory(deploymentProfile.SupportAccountId, FwFlexRequestType.Licensing,
                      (isCreateDP) ? DeploymentProfileRequestType.GETCNXDEPLOYMENTPROFILE.ToDescription() :
                      DeploymentProfileRequestType.EDITCNXDEPLOYMENTPROFILE.ToDescription(),
                      JsonConvert.SerializeObject(cnxDeploymentProfileRequest), JsonConvert.SerializeObject(deploymentProfileResponse), deploymentProfile.CreatedBy);
            }
            else if (deploymentProfile.Type == DeploymentProfileType.AI.ToDescription())
            {
                deploymentProfileResponse = string.IsNullOrEmpty(deploymentProfile.AuthCode)
                    ? _licenseLogic.GetDeploymentProfile(deploymentProfileRequest)
                    : deploymentProfile.isRenewal
                        ? _licenseLogic.RenewDeploymentProfile(deploymentProfileRequest)
                        : _licenseLogic.EditDeploymentProfile(deploymentProfileRequest);
                _flexDataAccess.LogAuditHistory(deploymentProfile.SupportAccountId, FwFlexRequestType.Licensing,
                    deploymentProfile.isRenewal
                        ? DeploymentProfileRequestType.RENEWDEPLOYMENTPROFILE.ToDescription()
                        : string.IsNullOrEmpty(deploymentProfile.AuthCode)
                            ? DeploymentProfileRequestType.GETDEPLOYMENTPROFILE.ToDescription()
                            : DeploymentProfileRequestType.EDITDEPLOYMENTPROFILE.ToDescription(),
                    JsonConvert.SerializeObject(deploymentProfileRequest),
                    JsonConvert.SerializeObject(deploymentProfileResponse), deploymentProfile.CreatedBy);
            }
            else
            {

                if (!deploymentProfile.isRenewal)
                {
                    deploymentProfileResponse = isCreateDP
                        ? _licenseLogic.GetDeploymentProfile(deploymentProfileRequest)
                        : _licenseLogic.EditDeploymentProfile(deploymentProfileRequest);

                    _flexDataAccess.LogAuditHistory(deploymentProfile.SupportAccountId, FwFlexRequestType.Licensing,
              (isCreateDP) ? DeploymentProfileRequestType.GETDEPLOYMENTPROFILE.ToDescription() :
              DeploymentProfileRequestType.EDITDEPLOYMENTPROFILE.ToDescription(),
              JsonConvert.SerializeObject(deploymentProfileRequest), JsonConvert.SerializeObject(deploymentProfileResponse), deploymentProfile.CreatedBy);
                }
                else
                {
                    deploymentProfileResponse = isCreateDP
                       ? _licenseLogic.GetDeploymentProfile(deploymentProfileRequest)
                       : _licenseLogic.RenewDeploymentProfile(deploymentProfileRequest);


                    _flexDataAccess.LogAuditHistory(deploymentProfile.SupportAccountId, FwFlexRequestType.Licensing,
                  (isCreateDP) ? DeploymentProfileRequestType.RENEWDEPLOYMENTPROFILE.ToDescription() :
                  DeploymentProfileRequestType.RENEWDEPLOYMENTPROFILE.ToDescription(),
                  JsonConvert.SerializeObject(deploymentProfileRequest), JsonConvert.SerializeObject(deploymentProfileResponse), deploymentProfile.CreatedBy);
                }
            }


            if (deploymentProfileResponse != null && deploymentProfileResponse.Success && !string.IsNullOrEmpty(deploymentProfileResponse.Authcode))
            {
                deploymentProfileRequest.Authcode = deploymentProfileResponse.Authcode;
                var oldDP = isCreateDP ? new DeploymentProfile() : _flexDataAccess.GetDeploymentProfile((long)deploymentProfile.Id);

                deploymentProfile.AuthCode = deploymentProfileResponse.Authcode;
                deploymentProfileStatus = _flexDataAccess.AddUpdateDeploymentProfile(deploymentProfile);

                var deploymentProfileList = new List<DeploymentProfile>();
                deploymentProfileList.Add(deploymentProfile);

                if (deploymentProfileStatus.Success)
                {
                    if (deploymentProfile.isRenewal)
                        PublishEvent(deploymentProfileList, FwFlexRequestType.TransferAllocate);
                    else
                        PublishEvent(deploymentProfileList, FwFlexRequestType.TokenAllocate);
                }

                if (deploymentProfile.IsAirgapEligible)
                {
                    if (isCreateDP)
                    {
                        //Call Prelicense3 for total number of firewalls
                        Task.Run(async () => {
                            await Task.Delay(60000);
                            var response = _licenseLogic.PrelicenceAirgapDP(deploymentProfileResponse.Authcode, deploymentProfile.VcpuQuantity.ToString(), deploymentProfile.FirewallQuantity);
                        });
                        
                    }
                    else
                    {
                        if (deploymentProfile.VcpuQuantity > oldDP.VcpuQuantity)
                        {
                            // Call License_Fetch
                            var deviceRequestParams = new FlexDeviceRequest
                            {
                                requestParameters = new Model.Flex.RequestParameters
                                {
                                    PaginationOffset = 0,
                                    PaginationCount = 1000,
                                    Sort = null,
                                    SearchField = "AuthorizationCode",
                                    SearchValue = deploymentProfile.AuthCode
                                }
                            };
                            var devices = _flexDataAccess.GetFlexDevices(deviceRequestParams, deploymentProfile.SupportAccountId, 1, deploymentProfile.UserAccountId);
                            Task.Run(async () => {
                                await Task.Delay(60000);
                                _licenseLogic.LicenseFetchAirgapDP(devices?.FlexDeviceList, deploymentProfile.VcpuQuantity);
                            });
                        }

                        if (deploymentProfile.FirewallQuantity > oldDP.FirewallQuantity)
                        {
                            //Call Prelicense3 for number of firewalls increased
                            var numberOfFW = deploymentProfile.FirewallQuantity - oldDP.FirewallQuantity;
                            Task.Run(async () => {
                                await Task.Delay(60000);
                                _licenseLogic.PrelicenceAirgapDP(deploymentProfileResponse.Authcode, deploymentProfile.VcpuQuantity.ToString(), numberOfFW);
                            });
                        }
                        

                    }
                }

            }
            else
            {
                deploymentProfileStatus = new DeploymentProfileStatus()
                {
                    AuthCode = deploymentProfileResponse.Authcode,
                    Success = false,
                    Message = deploymentProfileResponse.Message
                };
            }

            return deploymentProfileStatus;

        }

        public IEnumerable<CreditDataModel> GetAvailableCreditTypes(long sourceSupportAccountId, long targetSupportAccountId, long tokenId, long userAccountId)
        {
            var credits = new List<CreditDataModel>();

            var sourceCredits = GetFlexData(userAccountId, ApiConstants.TOKENID_TYPE, ApiConstants.TOKENID_VARIABLE, ApiConstants.TOKENID_VALUE, tokenId.ToString());

            var targetCredits = GetFlexData(userAccountId, ApiConstants.SUPPORTACCOUNTID_TYPE, ApiConstants.SUPPORTACCOUNTID_VARIABLE,
               ApiConstants.SUPPORTACCOUNTID_VALUE, targetSupportAccountId.ToString(), targetSupportAccountId);

            var credit = sourceCredits.Credits.Where(x => string.Compare(x.Status, CreditPoolStatus.Active.ToString(), StringComparison.OrdinalIgnoreCase) == 0).FirstOrDefault();


            var filteredTarget = targetCredits.Credits.Where(c => !string.IsNullOrEmpty(c.ExpirationDate) && !string.IsNullOrEmpty(credit.ExpirationDate) &&
                                 DateTime.Compare(DateTimeOffset.Parse(c.ExpirationDate).Date, DateTimeOffset.Parse(credit.ExpirationDate).Date) == 0 &&
                                 c.SupportType == credit.SupportType &&
                                 c.TokenId != credit.TokenId);

            var sourceCreditPool = sourceCredits.Credits.Where(x => x.TokenId == tokenId && string.Compare(x.Status, CreditPoolStatus.Active.ToString(), StringComparison.OrdinalIgnoreCase) == 0).FirstOrDefault();

            if (sourceCreditPool.ParentTokenId != 0)
            {
                filteredTarget = filteredTarget.Where(x => x.ParentTokenId == sourceCreditPool.ParentTokenId || x.TokenId == sourceCreditPool.ParentTokenId);
            }
            else
            {
                filteredTarget = filteredTarget.Where(x => x.ParentTokenId == tokenId);
            }

            Mapper.Map(filteredTarget, credits);

            EFedRampType fedRampType = _flexDataAccess.GetFedRampType(sourceSupportAccountId);

            credits.ForEach(i => i.AccountFedRampType = fedRampType);

            return credits;
        }

        public DeploymentProfileModel DeleteDeploymentProfile(long userAccountId, string authCode, string productId, long tokenId, string type, string panOs = null, string subType = null)
        {
            DeploymentProfileModel response = null;

            var deploymentProfileStatus = _flexDataAccess.GetDeploymentProfileStatus(authCode);
            if (deploymentProfileStatus == string.Empty || deploymentProfileStatus.Contains("Pending"))
            {
                return new DeploymentProfileModel
                {
                    Authcode = authCode,
                    Message = deploymentProfileStatus == string.Empty ? "An unexpected error has occurred." : "Deployment Profile status is pending. Cannot be deleted.",
                    Success = false
                };
            }

            if (type == DeploymentProfileType.CN.ToDescription() && ApiConstants.CNNonlegacyPanOs.Any(s => s.Contains(panOs)))
            {
                response = _licenseLogic.DeleteCnxDeploymentProfile(authCode);
            }
            else
            {
                response = _licenseLogic.DeleteDeploymentProfile(authCode);
            }

            _flexDataAccess.LogAuditHistory(0, FwFlexRequestType.Licensing,
                                          type == DeploymentProfileType.CN.ToDescription() && ApiConstants.CNNonlegacyPanOs.Any(s => s.Contains(panOs)) ? DeploymentProfileRequestType.DELETCNXDEPLOYMENTPROFILE.ToDescription() :
                                          DeploymentProfileRequestType.DELETDEPLOYMENTPROFILE.ToDescription(),
                                          authCode,
                                          JsonConvert.SerializeObject(response),
                                          userAccountId);

            if (response != null && response.Success)
            {
                _flexDataAccess.UpdateDeleteDeploymentProfileStatus(userAccountId, authCode, response.Message);

                var deploymentProfile = new DeploymentProfile
                {
                    AuthCode = authCode,
                    FirewallQuantity = 0,
                    VcpuQuantity = 0,
                    ProductId = productId,
                    TokenId = tokenId,
                    Type = type,
                    SubType = subType, 
                    PanOS = panOs,
                    AvgTxtRecordsPerDay = type == DeploymentProfileType.AI.ToString() 
                        && (subType == EDeploymentProfileSubType.API.ToString()
                        || subType == EDeploymentProfileSubType.RT.ToString()
                        || subType == EDeploymentProfileSubType.MS.ToString()
                        || subType == EDeploymentProfileSubType.GW.ToString()
                        || subType == EDeploymentProfileSubType.APIM.ToString()
                        || subType == EDeploymentProfileSubType.AES.ToString()
                        || subType == EDeploymentProfileSubType.CNGFW.ToString()
                        || subType == EDeploymentProfileSubType.SK.ToString()
                        || subType == EDeploymentProfileSubType.AF.ToString()
                        || subType == EDeploymentProfileSubType.SPM.ToString())
                        ? 0
                        : (int?)null,
                    MaxThreatSimulations = null, // REVERT: when AvgTxtRecordsPerDay hack is removed to - deploymentProfile.MaxThreatSimulations,
                    MaxScans = null, // REVERT: when AvgTxtRecordsPerDay hack is removed to - deploymentProfile.MaxScans
                    AttributesValues = GetZeroValueAttribute(type, subType)
                };

                _flexDataAccess.LogAuditHistory(0, FwFlexRequestType.TokenAllocate,
                                                string.Format("{0}{1}{2}", _projectId, "/", _topic),
                                                JsonConvert.SerializeObject(deploymentProfile),
                                                string.Empty,
                                                userAccountId);
                var deploymentProfileList = new List<DeploymentProfile>();
                deploymentProfileList.Add(deploymentProfile);
                PublishEvent(deploymentProfileList, FwFlexRequestType.TokenAllocate);
            }

            return response;
        }

        public FirewallRegisterResponse RegisterFireWall(FirewallRegisterRequest firewallRegisterRequest)
        {
            FirewallRegisterResponse firewallRegisterResponse = null;
            var response = _licenseLogic.RegisterFirewall(firewallRegisterRequest);
            _flexDataAccess.LogAuditHistory(firewallRegisterRequest.SupportAccountId,
                                            FwFlexRequestType.Licensing,
                                            response.Endpoint,
                                            JsonConvert.SerializeObject(firewallRegisterRequest),
                                            JsonConvert.SerializeObject(response.ServiceResponse),
                                            firewallRegisterRequest.UserAccountId);
            if (response != null && response.ServiceResponse.Any())
            {
                if (string.IsNullOrEmpty(response.ServiceResponse[0].errmsg))
                {
                    firewallRegisterResponse = new FirewallRegisterResponse
                    {
                        Success = true,
                        Message = "Firewall Registration Successful"
                    };
                }
                else
                {
                    firewallRegisterResponse = new FirewallRegisterResponse
                    {
                        Success = false,
                        Message = response.ServiceResponse[0].errmsg
                    };
                }
            }
            return firewallRegisterResponse;

        }

        public FlexDevices GetFlexDevices(FlexDeviceRequest flexDeviceRequest, long supportAccountId, long userAccountId, string type, string subType = null)
        {

            Firewallsummary firewallSummary = null;
            FlexDevices devices = new FlexDevices();
            if (flexDeviceRequest.requestParameters.SearchField == "LastCheckinDate")
            {
                FlexDeviceRequest flexDeviceRequestMax = new FlexDeviceRequest
                {
                    requestParameters = new Model.Flex.RequestParameters
                    {
                        PaginationOffset = 0,
                        PaginationCount = int.MaxValue,
                        SearchField = flexDeviceRequest.requestParameters.SearchField,
                        SearchValue = string.Empty,
                        Sort = flexDeviceRequest.requestParameters.Sort
                    }
                };
                string lastCheckinDate = flexDeviceRequest.requestParameters.SearchValue;
                var numberOfDays = int.Parse(flexDeviceRequest.requestParameters.SearchValue);
                firewallSummary = GetLastLoginDetails(userAccountId, supportAccountId, DateTime.Now.AddDays(-numberOfDays).ToString("yyyy-MM-dd"));

                devices = _flexDataAccess.GetFlexDevices(flexDeviceRequestMax, supportAccountId, type == DeploymentProfileType.VM.ToDescription() ? (int)DeploymentProfileType.VM :
                                                 (int)DeploymentProfileType.CN, userAccountId);
                var filteredDevices = devices?.FlexDeviceList.devices.Where(x => firewallSummary.firewallSummary.Any(y => y.serialNumber == x.SerialNumber)).ToList();
                if (filteredDevices != null && filteredDevices.Count > 0)
                {
                    // Calculate pagination parameters
                    int pageSize = flexDeviceRequest.requestParameters.PaginationCount > 0 ? flexDeviceRequest.requestParameters.PaginationCount : filteredDevices.Count;
                    int currentPage = flexDeviceRequest.requestParameters.PaginationOffset > 0 ? flexDeviceRequest.requestParameters.PaginationOffset : 1;
                    int totalElements = filteredDevices.Count;
                    int totalPages = pageSize > 0 ? (int)Math.Ceiling((double)totalElements / pageSize) : 1;

                    // Apply pagination to the filtered devices
                    var paginatedDevices = filteredDevices
                        .Skip((currentPage - 1) * pageSize)
                        .Take(pageSize)
                        .ToList();

                    // Update the devices object with paginated results
                    devices.FlexDeviceList.devices = paginatedDevices;
                    devices.FlexDeviceList.totalElementsCount = totalElements; // Keep total count of filtered items
                    devices.FlexDeviceList.totalPages = totalPages;
                }

                else
                {
                    // Handle empty results
                    devices.FlexDeviceList.devices = new List<FlexDevice>();
                    devices.FlexDeviceList.totalElementsCount = 0;
                    devices.FlexDeviceList.totalPages = 0;
                }

            }


            else
            {
                devices = _flexDataAccess.GetFlexDevices(flexDeviceRequest, supportAccountId, type == DeploymentProfileType.VM.ToDescription() ? (int)DeploymentProfileType.VM :
                                                 (int)DeploymentProfileType.CN, userAccountId);
            }

            var creditQuantityUsed = GetCreditQuantityUsed(userAccountId, supportAccountId);


            AddLicenesAndCreditQuantityUsedToDevices(devices.FlexDeviceList.devices, creditQuantityUsed, firewallSummary);
            return devices;
        }

        public PanoramaDevices GetPanoramas(FlexDeviceRequest flexDeviceRequest, long supportAccountId)
        {
            var panoramas = _flexDataAccess.GetPanorams(flexDeviceRequest, supportAccountId);
            AddLicenesToDevices(panoramas.PanoromaDevices.panoramas);
            return panoramas;
        }

        // TO DO get allocations by support account and map using auth code to get credits used
        //foreach (var device in devices)
        //{
        //    if (responsePayload == null || responsePayload.Credits == null) break;
        //    var credit = responsePayload.Credits.FirstOrDefault(x => x.SupportAccountId == device.SupportAccountId);
        //    if (credit != null)
        //    {
        //        device.CreditsUsed = credit.NetConsumption;
        //    }
        //}
        public double GetTokensConsumed(long userAccountId, long supportAccountId, string authCode)
        {
            double tokensConsumed = 0;
            var graphQLRequest = string.Format(ApiConstants.TOKEN_QUANTITY_ALLOCATION, authCode);

            var responsePayload = GetTokensConsumedDetails(userAccountId, supportAccountId, graphQLRequest);
            if (responsePayload != null && responsePayload.QuantityAllocation != null)
            {
                tokensConsumed = responsePayload.QuantityAllocation.Sum(q => q.QuantityConsumed);
            }
            return tokensConsumed;
        }

        public DeploymentProfileList GetDeploymentProfiles(long userAccountId, long supportAccountId, long tokenId, string poolExpirationDate)
        {
            var creditPoolExpirationDate = DateTime.Parse(poolExpirationDate);
            string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(_licensingUserName + ":" + _licensingPassword));
            var graphQLRequest = string.Format(ApiConstants.TOKEN_SUMMARY_QUANTITY_ALLOCATION, tokenId);
            var responsePayload = TokenHelper.GetFlexData<CreditDetail>(_licensingApi, credentials, "POST", _tokenServiceThumbprint, graphQLRequest, userAccountId, supportAccountId);
            var deploymentProfileList = _flexDataAccess.GetDeploymentProfiles(supportAccountId, tokenId);

            var creditPoolRenewalStatus = GetCreditDataModel(tokenId.ToString(), userAccountId).FirstOrDefault()?.CreditPoolRenewalStatus;

            if (responsePayload != null && responsePayload.QuantityAllocation != null && deploymentProfileList.profiles != null)
            {
                deploymentProfileList.profiles.ForEach(x =>
                {
                    var quantityAllocation = responsePayload.QuantityAllocation.Where(y => y.AllocationId == x.AuthCode);
                    if (quantityAllocation != null)
                    {
                        x.TokensAllocated = quantityAllocation.Sum(y => y.QuantityAllocated);
                        x.TokensConsumed = quantityAllocation.Sum(y => y.QuantityConsumed);
                        x.ConsumptionRenewalStatus = quantityAllocation?.FirstOrDefault()?.ConsumptionRenewalStatus;
                    }
                    x.CreditPoolRenewalStatus = creditPoolRenewalStatus;
                    x.IsRenewalPending = IsRenewalPending(creditPoolExpirationDate, x.ExpirationDate);
                    x.GenType = responsePayload.GenType;
                });
            }


            if (deploymentProfileList.profiles != null)
            {
                var pendingRenewalDeploymentProfiles = GetPendingRenewalDeploymentProfiles(deploymentProfileList);

                if (pendingRenewalDeploymentProfiles.Count > 0)
                {
                    var authCodeList = pendingRenewalDeploymentProfiles.Select(x => x.AuthCode).ToList();
                    deploymentProfileList.profiles = deploymentProfileList.profiles.Where(x => !authCodeList.Contains(x.AuthCode)).ToList();
                }
            }

            return deploymentProfileList;
        }

        public bool IsProfileNameExists(string profileName, long? profileId)
        {
            return _flexDataAccess.IsProfileNameExists(profileName, profileId);
        }

        public DeploymentProfile GetDeploymentProfile(long deploymentProfileId)
        {
            return _flexDataAccess.GetDeploymentProfile(deploymentProfileId);
        }

        public DeploymentProfile GetDeploymentProfile(string authCode, long userAccountId, long supportAccountId)
        {
            var deploymentProfile = _flexDataAccess.GetDeploymentProfile(authCode);

            string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(_licensingUserName + ":" + _licensingPassword));
            var graphQLRequest = string.Format(ApiConstants.TOKEN_SUMMARY_QUANTITY_ALLOCATION, deploymentProfile.TokenId);
            var responsePayload = TokenHelper.GetFlexData<CreditDetail>(_licensingApi, credentials, "POST", _tokenServiceThumbprint, graphQLRequest, userAccountId, supportAccountId);

            if (responsePayload != null && responsePayload.QuantityAllocation != null && deploymentProfile != null)
            {
                var quantityAllocation = responsePayload.QuantityAllocation.Where(y => y.AllocationId == authCode);
                if (quantityAllocation != null)
                {
                    deploymentProfile.TokensAllocated = Math.Round(quantityAllocation.Sum(y => y.QuantityAllocated), 2);
                    deploymentProfile.TokensConsumed = Math.Round(quantityAllocation.Sum(y => y.QuantityConsumed), 2);
                }
            }
            return deploymentProfile;
        }

        public TokenAllocationDetail GetAllocationDetail(long tokenId, string productId, long userAccountId, long? deploymentProfileId)
        {
            var tokenAvailable = _flexDataAccess.GetAllocationDetail(tokenId, deploymentProfileId);

            var credits = GetCreditDataModel(tokenId.ToString(), userAccountId);

            if (credits != null && credits.Count > 0)
            {
                tokenAvailable.TotalAvailableTokens = credits.Where(x => x.ProductId == productId).FirstOrDefault().NetBalance;
                tokenAvailable.NetBalance = credits.Where(x => x.ProductId == productId).FirstOrDefault().NetBalance - tokenAvailable.NetBalance;
            }

            return tokenAvailable;
        }

        public PanoramaProvisionResponse PanoramaProvision(PanoramaProvisionModel model)
        {
            var request = new PanoramaProvisionRequest();
            Mapper.Map(model, request);
            var response = LicenseApis.PanoramaProvision(request);

            if (response.Status)
            {
                var deploymentProfileList = new List<DeploymentProfile>();
                response.SerialNumber.ForEach(x =>
                {
                    var deploymentProfile = new DeploymentProfile()
                    {
                        AuthCode = request.AuthCode,
                        ProductId = model.ProductId,
                        TokenId = model.TokenId,
                        Type = model.Type,
                        SubType = model.SubType, 
                        PanOS = model.PanOs = model.PanOs,
                        Panorama = model.Panorama?.ToList(), 
                        ReferenceCustomData = new Dictionary<string, string>() { { "serialNumber", x } },
                        FedRampType = model.FedRampType
                    };
                    deploymentProfileList.Add(deploymentProfile);
                });

                PublishEvent(deploymentProfileList, FwFlexRequestType.Provision);
            }

            return response;
        }

        public List<PanoramasDeleteResponse> DeletePanoramas(PanoramasDeleteRequest request)
        {

           
            string serialNumber = string.Empty;
            if (request.serialNumbers != null)
            {
                serialNumber = string.Join(",", request.serialNumbers.Select(x => x.SerialNumber.ToString()));

            }                        
            var response = _licenseLogic.DeletePanoramas(serialNumber);

            _flexDataAccess.LogAuditHistory(request.SupportAccountId,
                                          FwFlexRequestType.Licensing,
                                          response?.FirstOrDefault()?.Endpoint,
                                          JsonConvert.SerializeObject(request),
                                          JsonConvert.SerializeObject(response),
                                          request.UserAccountId);

            var failedSerialNumbers = response.Where(x => x.success == false).Select(x => x.serialNumber).ToList();

            if (response != null)
            {
                var deploymentProfileList = new List<DeploymentProfile>();
                List<string> SerialNumbers = serialNumber.Split(',').ToList<string>();
                SerialNumbers?.ToList().ForEach(x =>
                {
                    if (!failedSerialNumbers.Contains(x))
                    {
                        var deploymentProfile = new DeploymentProfile()
                        {
                            AuthCode = request.AuthCode,
                            ProductId = request.ProductId,
                            TokenId = request.TokenId,
                            Type = request.Type,
                            PanOS = request.PanOs,
                            ReferenceCustomData = new Dictionary<string, string>() { { "serialNumber", x } },
                            FedRampType = request.FedRampType,
                        };
                        deploymentProfileList.Add(deploymentProfile);
                    };
                });
                if(deploymentProfileList.Count > 0)
                PublishEvent(deploymentProfileList, FwFlexRequestType.DeletePanorama);
            }

            return response;
        }

        public IEnumerable<PanoramaProvisionedDevice> GetProvisionedPanoramas(string authCode)
        {
            return _flexDataAccess.GetProvisionedPanoramas(authCode);
        }

        public List<string> GetSubscriptionsByUseCase(List<int> useCase)
        {
            return _flexDataAccess.GetSubscriptionsByUseCase(useCase);
        }

        public List<string> GetSubscriptionsByDeploymentProfileId(long deploymentProfileId)
        {
            return _flexDataAccess.GetSubscriptionsByDeploymentProfileId(deploymentProfileId);
        }


        public DownloadLicenseResponse DownloadPanoramaLicense(long licenseId, long userAccountId)
        {
            DownloadLicenseResponse downloadLicenseResponse = null;
            try
            {
                var licenseKey = _licenseLogic.GetLicenseKeybyId(licenseId, userAccountId);
                downloadLicenseResponse = new DownloadLicenseResponse()
                {
                    LicenseKey = licenseKey.Key.Replace("\n", Environment.NewLine),
                    Success = true,
                    LicenseName = licenseKey.Name
                };
            }
            catch (Exception exception)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(exception);
            }
            return downloadLicenseResponse;
        }


        public List<FwSerialAndSubscription> GetFirewalls(long supportAccountId, long userAccountId, string authCode, string type, string subType = null)
        {
            return _flexDataAccess.GetFirewalls(supportAccountId, userAccountId, authCode, type, subType);
        }

        public List<string> DeactivateFirewall(FirewallDeactivationRequest firewallDeactivationRequest, string userEmailId)
        {
            // whether the consumption exits or not , A call is required to the licensing
            List<string> serialNumbers = firewallDeactivationRequest.SerialNumber.Distinct().ToList();
            List<WarmPoolDeviceState> devicesStates = _flexDataAccess.GetWarmPoolDeviceState(serialNumbers);
            var failedSerialNumbers = _flexDataAccess.DeactivateFirewall(serialNumbers, userEmailId);
            devicesStates = devicesStates.Where(i => failedSerialNumbers.All(e => e != i.SerialNumber)).ToList(); // exclude failed SNs
            foreach (var item in serialNumbers)
            {
                if (!_flexDataAccess.IsConsumptionReported(item, firewallDeactivationRequest.AuthCode, _retryUrlService))
                {
                    firewallDeactivationRequest.SerialNumber.Remove(item);
                }
            }

            var deploymentProfileList = new List<DeploymentProfile>();
            serialNumbers.ForEach(x =>
            {
                if (!failedSerialNumbers.Contains(x)) // do not publish failed deactivation devices
                {
                    var deploymentProfile = new DeploymentProfile()
                    {
                        AuthCode = firewallDeactivationRequest.AuthCode,
                        ProductId = firewallDeactivationRequest.ProductId,
                        TokenId = firewallDeactivationRequest.TokenId,
                        Type = firewallDeactivationRequest.Type,
                        SubType = firewallDeactivationRequest.SubType,
                        ReferenceCustomData = new Dictionary<string, string>() { { "serialNumber", x } },
                        Panorama = new List<string>(),
                        FirewallQuantity = 0,
                        VcpuQuantity = 0,
                        PanOS = firewallDeactivationRequest.PanOs, 
                        State = devicesStates.FirstOrDefault(i => i.SerialNumber == x)?.TargetState ?? DEVICE_STATE_ACTIVE, 
                        AvgTxtRecordsPerDay = 0,
                        AttributesValues = GetZeroValueAttribute(firewallDeactivationRequest.Type, firewallDeactivationRequest.SubType), 
                        FedRampType = firewallDeactivationRequest.FedRampType
                    };
                    deploymentProfileList.Add(deploymentProfile);
                }
            });

            if (deploymentProfileList.Count > 0)
            {
                deploymentProfileList.ForEach(x =>
                {
                    _flexDataAccess.UpdateProfileStatusOnDeactivate(firewallDeactivationRequest.UserAccountId, x.AuthCode);
                });

                PublishEvent(deploymentProfileList, FwFlexRequestType.Delete);
            }

            _flexDataAccess.UpdateWarmPoolDeviceState(devicesStates, userEmailId);
            return failedSerialNumbers;
        }

        public DownloadLicenseResponse DownloadLicenseKey(long licenseId, long userAccountId)
        {
            return _licenseLogic.DownloadLicenseKey(licenseId, userAccountId);
        }

        public IEnumerable<DeploymentProfileAudit> GetDeploymentProfileAudit(string tokenId)
        {
            return _flexDataAccess.GetDeploymentProfileAudit(tokenId);
        }

        public IEnumerable<DeploymentProfileAuditDetail> GetDeploymentProfileAuditDetail(long auditId)
        {
            return _flexDataAccess.GetDeploymentProfileAuditDetail(auditId);
        }

        public List<PanoramaMigration> GetPanoramasForMigration(long supportAccountId, string authCode, string type, string subType = null)
        {
            return _flexDataAccess.GetPanoramasForMigration(supportAccountId, authCode, type);
        }


        public List<PanoramaMigrationDetail> GetPanoramaMigrationDetail(long deviceId, string authCode)
        {
            return _flexDataAccess.GetPanoramaMigrationDetail(deviceId, authCode);
        }


        public List<RepurposePanoramasResponse> MigratePanoramas(RepurposePanoramasRequest request)
        {
            var response = _licenseLogic.RepurposePanoramas(request);

            _flexDataAccess.LogAuditHistory(request.SupportAccountId,
                                            FwFlexRequestType.Licensing,
                                            response?.FirstOrDefault()?.Endpoint,
                                            JsonConvert.SerializeObject(request?.RepurposeReqInfo),
                                            JsonConvert.SerializeObject(response),
                                            request.UserAccountId);

            if (request.RepurposeReqInfo != null && response != null)
            {
                var deploymentProfileList = new List<DeploymentProfile>();
                request.RepurposeReqInfo.ToList().ForEach(x =>
                {
                    if (response.Find(y => y.serialNumber == x.SerialNumber && y.success == true) != null)
                    {
                        var deploymentProfile = new DeploymentProfile()
                        {
                            AuthCode = request.AuthCode,
                            SupportAccountId = request.SupportAccountId,
                            ProductId = request.ProductId,
                            TokenId = request.TokenId,
                            Type = request.Type,
                            PanOS = request.PanOs,
                            SourceAuthcode = !string.IsNullOrEmpty(x.SourceAuthCode) ? x.SourceAuthCode.StartsWith("D") ? x.SourceAuthCode : string.Empty: string.Empty,
                            ReferenceCustomData = new Dictionary<string, string>() { { "serialNumber", x.SerialNumber } },
                            FedRampType = request.FedRampType,
                        };
                        deploymentProfileList.Add(deploymentProfile);
                    }
                });

                if (deploymentProfileList.Count > 0)
                {
                    PublishEvent(deploymentProfileList, FwFlexRequestType.MigratePanorama);
                }
            }

            return response;

        }

        public IList<CreditPool> CreditPoolsForTransfer(long creditPoolId, long supportAccountId, long userAccountId)
        {
            var creditPoolList = new List<CreditPool>();
            var responsePayload = GetFlexData(userAccountId, ApiConstants.SUPPORTACCOUNTID_TYPE, ApiConstants.SUPPORTACCOUNTID_VARIABLE,
               ApiConstants.SUPPORTACCOUNTID_VALUE, supportAccountId.ToString(), supportAccountId);

            if (responsePayload != null && responsePayload.Credits != null)
            {
                creditPoolList = responsePayload.Credits.Where(x => x.TokenId != creditPoolId
                                                            && !x.Sku.Contains("SUB")
                                                            && string.Compare(x.Status, CreditPoolStatus.Active.ToString(), StringComparison.OrdinalIgnoreCase) == 0
                                                            && DateTime.Compare(DateTimeOffset.Parse(x.ExpirationDate).Date, DateTimeOffset.Now.Date) > 0
                                                            && x.SupportAccountId == supportAccountId) // TODO: Sesh said that transfer DP to diff CSP ID Credit Pool may have consiquenses in HUB
                                       .Select(x => new CreditPool
                                       {
                                           CreditName = x.CreditName,
                                           TokenId = x.TokenId,
                                           ExpirationDate = DateTimeOffset.Parse(x.ExpirationDate).Date,
                                           NetBalance = x.NetBalance,
                                           SupportType = x.SupportType,
                                           ProductId = x.ProductId,
                                           Sku = x.Sku,
                                           IsAirgapEligible = x.IsAirgapEligible
                                       })
                                       .ToList();
                /* TODO: cleanup if DP transfer is allowed to a CP of diff CSP ID; complications in HUB?
                // Get FedRAMP Type of the source CSP ID
                EFedRampType fedRampType = EFedRampType.Moderate; // _flexDataAccess.GetFedRampType(supportAccountId);

                if (fedRampType > EFedRampType.Commercial) // FedRAMP DP can be transfered only to a Credit Pool of a matching FedRAMP CSP ID or to another source CSP ID Credit Pool
                {
                    // build exclusion list
                }
                */
            }
            return creditPoolList;
        }

        public TransferDeploymentProfileResponse TransferDeploymentProfiles(TransferDeploymentProfilesRequest transferDeploymentProfilesRequest)
        {
            string supportType = supportTypeMapping.FirstOrDefault(x => x.Key == transferDeploymentProfilesRequest.DestSupportType).Value;
            var ascAccountInfo = new AscAccountInfo();
            var tansferDeploymentProfilesLicensingRequest = new TransferDeploymentProfilesLicensingRequest
            {
                Config = GetAllocationConfiguration(transferDeploymentProfilesRequest.deploymentProfile, true),
                DestCreditPool = transferDeploymentProfilesRequest.DestCreditPool,
                DestExpirationDate = transferDeploymentProfilesRequest.DestExpirationDate,
                DestSupportType = transferDeploymentProfilesRequest.DestSupportType,
                DestProductId = transferDeploymentProfilesRequest.DestProductId,
                SourceAuthCode = transferDeploymentProfilesRequest.deploymentProfile.AuthCode,
                PartId = transferDeploymentProfilesRequest.deploymentProfile.Type == DeploymentProfileType.CN.ToDescription() && transferDeploymentProfilesRequest.deploymentProfile.PanOS == ApiConstants.CNLegacyPanOs ? string.Format("PAN-CN-X-{0}-{1}", transferDeploymentProfilesRequest.deploymentProfile.Subscriptions[0], supportTypeMapping.FirstOrDefault(x => x.Key == transferDeploymentProfilesRequest.deploymentProfile.SupportType).Value) : transferDeploymentProfilesRequest.DestSku,
            };

            if (supportType == "BKLN" || supportType == "BKLNUSG")
            {
                var creditDetail = GetFlexData(transferDeploymentProfilesRequest.UserAccountId, ApiConstants.TOKENID_TYPE, ApiConstants.TOKENID_VARIABLE, ApiConstants.TOKENID_VALUE,
                    transferDeploymentProfilesRequest.DestCreditPool.ToString());
                if (creditDetail != null && creditDetail.Credits.Any())
                {
                    ascAccountInfo = _flexDataAccess.GetAscAccountInfo(creditDetail.Credits.FirstOrDefault().AscAccountId);
                    tansferDeploymentProfilesLicensingRequest.AscId = ascAccountInfo.AscId;
                    tansferDeploymentProfilesLicensingRequest.AscName = ascAccountInfo.AscName;
                }
            }

            var licesingResponse = _licenseLogic.TransferDeploymentProfiles(tansferDeploymentProfilesLicensingRequest);
            _flexDataAccess.LogAuditHistory(transferDeploymentProfilesRequest.deploymentProfile.SupportAccountId,
                                         FwFlexRequestType.Licensing,
                                         licesingResponse?.Endpoint,
                                         JsonConvert.SerializeObject(tansferDeploymentProfilesLicensingRequest),
                                         JsonConvert.SerializeObject(licesingResponse),
                                         transferDeploymentProfilesRequest.deploymentProfile.CreatedBy);

            if (licesingResponse.Success == true)
            {
                _flexDataAccess.TransferDeploymentDeploymentProfile(transferDeploymentProfilesRequest.deploymentProfile.Id,
                                                                    transferDeploymentProfilesRequest.DestCreditPool,
                                                                    transferDeploymentProfilesRequest.deploymentProfile.TokensAllocated,
                                                                    transferDeploymentProfilesRequest.UserAccountId);
            }
            return licesingResponse;
        }

        public DeploymentProfileList GetRenewalDeploymentProfiles(long userAccountId, long supportAccountId, long tokenId)
        {
            string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(_licensingUserName + ":" + _licensingPassword));
            var graphQLRequest = string.Format(ApiConstants.TOKEN_SUMMARY_QUANTITY_ALLOCATION, tokenId);
            var responsePayload = TokenHelper.GetFlexData<CreditDetail>(_licensingApi,
                                                                        credentials,
                                                                        "POST",
                                                                        _tokenServiceThumbprint,
                                                                        graphQLRequest,
                                                                        userAccountId,
                                                                        supportAccountId);

            var deploymentProfileList = _flexDataAccess.GetDeploymentProfiles(supportAccountId, tokenId);
            var creditPoolRenewalStatus = GetCreditDataModel(tokenId.ToString(), userAccountId).FirstOrDefault()?.CreditPoolRenewalStatus;

            if (responsePayload != null && responsePayload.QuantityAllocation != null && deploymentProfileList.profiles != null)
            {
                deploymentProfileList.profiles.ForEach(x =>
                {
                    var quantityAllocation = responsePayload.QuantityAllocation.Where(y => y.AllocationId == x.AuthCode);
                    if (quantityAllocation != null)
                    {
                        x.TokensAllocated = quantityAllocation.Sum(y => y.QuantityAllocated);
                        x.TokensConsumed = quantityAllocation.Sum(y => y.QuantityConsumed);
                        x.ConsumptionRenewalStatus = quantityAllocation?.FirstOrDefault()?.ConsumptionRenewalStatus;
                    }
                    x.CreditPoolRenewalStatus = creditPoolRenewalStatus;
                });
            }

            if (deploymentProfileList.profiles != null)
            {
                var pendingRenewalProfiles = GetPendingRenewalDeploymentProfiles(deploymentProfileList);

                if (pendingRenewalProfiles != null && pendingRenewalProfiles.Any())
                {
                    deploymentProfileList.profiles = pendingRenewalProfiles.ToList();
                }
                else
                {
                    deploymentProfileList.profiles = new List<DeploymentProfile>();
                }
            }

            return deploymentProfileList;
        }

        public IEnumerable<PanOsVersion> GetPanOsVersions(string panOs, PanOsVersions PanOsVersionsList)
        {
            var panOsVersions = new List<PanOsVersion>();
            var panOsversion = !String.IsNullOrEmpty(panOs) ? panOs.Substring(0, panOs.IndexOf("_")) : "";
            Regex pattern = new Regex(@"\d+(\.\d+){2,}");

            try
            {
                if (PanOsVersionsList?.PanOsVersion != null)
                {
                    foreach (var item in PanOsVersionsList.PanOsVersion)
                    {
                        if (string.IsNullOrEmpty(panOs))
                        {
                            var version = new PanOsVersion()
                            {
                                PanOsVersionLabel = item.ToString(),
                                PanOsVersionValue = item.ToString()
                            };
                            panOsVersions.Add(version);

                        }
                        else
                        {
                            Match m = pattern.Match(item);
                            string version1 = m.Value;
                            string s1 = string.Join("", version1.Split('.').Take(3)
                                                  .Select(x => x.PadLeft(2, '0')));

                            string panos = string.Join("", panOsversion.Split('.')
                                                .Select(x => x.PadLeft(2, '0')));
                            if (panOs.Contains("or-below") && System.Convert.ToInt64(s1) <= System.Convert.ToInt64(panos))
                            {
                                var version = new PanOsVersion()
                                {
                                    PanOsVersionLabel = item.ToString(),
                                    PanOsVersionValue = item.ToString()
                                };
                                panOsVersions.Add(version);
                            }
                            if ((panOs.Contains("or-above") || panOs.Contains("and-above")) && System.Convert.ToInt64(s1) >= System.Convert.ToInt64(panos))
                            {
                                var version = new PanOsVersion()
                                {
                                    PanOsVersionLabel = item.ToString(),
                                    PanOsVersionValue = item.ToString()
                                };
                                panOsVersions.Add(version);
                            }

                        }
                    }
                    panOsVersions?.OrderBy(x => x.PanOsVersionValue).ToList();
                }

            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return panOsVersions;

        }

        public UserAccount GetUserAccountFromEmail(string userEmail)
        {
            UserAccountDataAccess userAccountDataAccess = new UserAccountDataAccess();

            return userAccountDataAccess.ReadUserByEmailId(userEmail);
        }

        public dynamic GetCloudCreditDetails(long supportAccountId, long? creditPoolId = null, int page = 0, int size = 10)
        {
            dynamic result = null;
            try
            {
                var method = HttpMethod.Get.Method;
                var baseUrl = string.Format(ApiConstants.CloudCreditDetailsEndpoint, _applicationSettings.EntitlementServiceBffHost, supportAccountId, 
                    creditPoolId, page, size);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.EntitlementBffThumbprint, string.Empty, true);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }

        public dynamic FirewallRegistration(FirewallRegistrationRequest firewallRegistrationRequest)
        {
            dynamic result = null;
            try
            {
                var method = HttpMethod.Post.Method;
                var jsonPayload = JsonConvert.SerializeObject(firewallRegistrationRequest);
                var baseUrl = string.Format(ApiConstants.FirewallRegistrationEndpoint, _applicationSettings.EntitlementServiceBffHost);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.EntitlementBffThumbprint, jsonPayload, true);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }

        public dynamic GetPanOSVersions(long userAccountId, long supportAccountId, string panOSVersion = "")
        {
            dynamic result = null;
            string baseUrl = string.Empty;
            try
            {
                var method = HttpMethod.Get.Method;
                if (string.IsNullOrEmpty(panOSVersion))
                {
                    baseUrl = string.Format(ApiConstants.PanOSVersionWithoutVersionEndpoint, _applicationSettings.EntitlementServiceBffHost, supportAccountId, userAccountId);
                }
                else
                {
                    baseUrl = string.Format(ApiConstants.PanOSVersionWithVersionEndpoint, _applicationSettings.EntitlementServiceBffHost, supportAccountId, userAccountId, panOSVersion);
                }
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.EntitlementBffThumbprint, string.Empty, true);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }

        #region Private method

        private List<DeploymentProfile> GetPendingRenewalDeploymentProfiles(DeploymentProfileList deploymentProfileList)
        {
            return deploymentProfileList.profiles.Where(x => string.Compare(x.CreditPoolRenewalStatus, PoolRenewalStatus.None.ToString(), StringComparison.OrdinalIgnoreCase) != 0
                                && string.Compare(x.CreditPoolRenewalStatus, PoolRenewalStatus.Confirmed.ToString(), StringComparison.OrdinalIgnoreCase) != 0
                               && string.Compare(x.ConsumptionRenewalStatus, ConsumptionRenewalStatus.Confirmed.ToString(), StringComparison.OrdinalIgnoreCase) == 0 || x.IsGraceAllowed).ToList();
        }

        private bool IsRenewalConfirmed(CreditData creditData, long tokenId)
        {


            var activeNone = creditData.Credits.Where(x => x.TokenId == tokenId &&
                                            string.Compare(x.Status, CreditPoolStatus.Active.ToString(), StringComparison.OrdinalIgnoreCase) == 0 &&
                                            string.Compare(x.CreditPoolRenewalStatus, PoolRenewalStatus.None.ToString(), StringComparison.OrdinalIgnoreCase) == 0);

            var activeActive = creditData.Credits.Where(x => x.TokenId == tokenId &&
                                            string.Compare(x.Status, CreditPoolStatus.Active.ToString(), StringComparison.OrdinalIgnoreCase) == 0 &&
                                            string.Compare(x.CreditPoolRenewalStatus, PoolRenewalStatus.Active.ToString(), StringComparison.OrdinalIgnoreCase) == 0);

            var inactiveConfirmed = creditData.Credits.Where(x => x.TokenId == tokenId &&
                                            string.Compare(x.Status, CreditPoolStatus.Inactive.ToString(), StringComparison.OrdinalIgnoreCase) == 0 &&
                                            string.Compare(x.CreditPoolRenewalStatus, PoolRenewalStatus.Confirmed.ToString(), StringComparison.OrdinalIgnoreCase) == 0);

            return ((activeNone.Count() > 0 || activeActive.Count() > 0) && inactiveConfirmed.Count() > 0) ? true : false;
        }

        private void CheckandAssignCreditAdminRole(long supportAccountId, long userAccountId)
        {
            _flexDataAccess.CheckandAssignCreditAdminRole(supportAccountId, userAccountId);
        }

        private RequestDetail GetRequestDetail(string transactionType, dynamic request)
        {
            var requestDetail = new RequestDetail()
            {
                RequestUrl = string.Format("{0}{1}", _usageApi, "?transactionType=" + transactionType),
                Credential = Convert.ToBase64String(Encoding.ASCII.GetBytes(_usageApiUserName + ":" + _usageApiPassword)),
                JsonRequest = JsonConvert.SerializeObject(request)
            };
            return requestDetail;
        }

        private void PublishEvent(List<DeploymentProfile> deploymentProfiles, FwFlexRequestType fwFlexRequestType)
        {
            var allocateTokenList = new List<AllocateToken>();
            foreach (var deploymentProfile in deploymentProfiles)
            {

                string[] subscriptionList = new string[] { };
                if (deploymentProfile.Subscriptions != null && deploymentProfile.Subscriptions.Any())
                {
                    if (deploymentProfile.Type == DeploymentProfileType.CN.ToDescription() && deploymentProfile.PanOS == ApiConstants.CNLegacyPanOs)
                    {
                        subscriptionList = GetSubscriptionsByUseCase(deploymentProfile.UseCaseId).ToArray();
                    }
                    else
                    {
                        subscriptionList = deploymentProfile.Subscriptions;
                    }
                }
                /* TODO: delete because it's not in use anymore
                var allocationConfigurations = new AllocationConfiguration()
                {
                    QuantityFirewall = GetFirewallQuantity(deploymentProfile),
                    QuantityCPU = deploymentProfile.Type == DeploymentProfileType.CN.ToDescription() && deploymentProfile.PanOS == ApiConstants.CNLegacyPanOs ? 0 : deploymentProfile.VcpuQuantity,
                    FirewallType = deploymentProfile.Type,
                    QuantityVSYS = deploymentProfile.Type == DeploymentProfileType.VM.ToDescription() ? deploymentProfile.VSysQuantity :0,
                    Panorama = (deploymentProfile.Panorama != null && deploymentProfile.Panorama.Count > 0) ? deploymentProfile.Panorama.ConvertAll(delegate (string x)
                    {
                        return ((PanoramaType)Enum.Parse(typeof(PanoramaType), x)).ToDescription();
                    }) : deploymentProfile.Panorama,
                    Subscriptions = subscriptionList,
                    KubernatesNodes = deploymentProfile.Type == DeploymentProfileType.CN.ToDescription() && deploymentProfile.PanOS == ApiConstants.CNLegacyPanOs ? deploymentProfile.FirewallQuantity : deploymentProfile.KubernatesNodes,
                    AvgTxtRecordsPerDay = deploymentProfile.AvgTxtRecordsPerDay,
                    SubType = deploymentProfile.SubType,
                    MaxThreatSimulations = deploymentProfile.MaxThreatSimulations, 
                    MaxScans = deploymentProfile.MaxScans,
                    AttributesValues = deploymentProfile.AttributesValues,
                    FedRampType = deploymentProfile.FedRampType.ToString()
                };
                */
                if (fwFlexRequestType == FwFlexRequestType.TransferAllocate)
                {
                    allocateTokenList.Add(new AllocateToken
                    {
                        TargetTokenId = deploymentProfile.TokenId,
                        TokenId = deploymentProfile.TokenId,
                        Authcode = deploymentProfile.AuthCode,
                        ProductId = deploymentProfile.ProductId,
                        FirewallType = deploymentProfile.Type,
                        FirewallSubType = deploymentProfile.SubType, 
                        AllocationConfigurations = GetAllocationConfiguration(deploymentProfile),
                        ReferenceTransactionId = Convert.ToBase64String(Guid.NewGuid().ToByteArray()),
                        ReferenceCustomData = JsonConvert.SerializeObject(deploymentProfile.ReferenceCustomData),
                        TransactionDate = DateTime.UtcNow,
                        UpdateConsumedRecords = true,
                        State = deploymentProfile.State,
                        FedRampType = deploymentProfile.FedRampType.ToString(),
                        AttributesValues = deploymentProfile.AttributesValues
                    });
                }
                else
                {

                    allocateTokenList.Add(new AllocateToken
                    {
                        SourceTokenId = deploymentProfile.SourceTokenId,
                        TokenId = deploymentProfile.TokenId,
                        SupportAccountId = deploymentProfile.SupportAccountId,
                        Authcode = deploymentProfile.AuthCode,
                        ProductId = deploymentProfile.ProductId,
                        FirewallType = deploymentProfile.Type,
                        FirewallSubType = deploymentProfile.SubType,
                        AllocationConfigurations = GetAllocationConfiguration(deploymentProfile),
                        ReferenceTransactionId = Convert.ToBase64String(Guid.NewGuid().ToByteArray()),
                        ReferenceCustomData = JsonConvert.SerializeObject(deploymentProfile.ReferenceCustomData),
                        TransactionDate = DateTime.UtcNow,
                        SourceAuthcode = deploymentProfile.SourceAuthcode, 
                        State = deploymentProfile.State, 
                        FedRampType = deploymentProfile.FedRampType.ToString(),
                        AttributesValues = deploymentProfile.AttributesValues
                    });
                }
            }

            var request = new FlexBaseRequestPayload<AllocateToken>
            {
                TransactionType = fwFlexRequestType.ToDescription(),
                Data = allocateTokenList
            };

            _flexDataAccess.LogAuditHistory(deploymentProfiles[0].SupportAccountId, FwFlexRequestType.TokenAllocate, string.Format("{0}{1}{2}", _projectId, "/", _topic), JsonConvert.SerializeObject(request), "", deploymentProfiles[0].CreatedBy);
            EventPublisher eventPublisher = new EventPublisher();
            eventPublisher.PublishCustomEvent(request, _projectId, _topic);
        }

        private int GetFirewallQuantity(DeploymentProfile deploymentProfile, bool isFromTransferDp = false)
        {
            if (deploymentProfile.Type == DeploymentProfileType.CN.ToDescription() && deploymentProfile.PanOS == ApiConstants.CNLegacyPanOs)
                return 0; // Setting Firewall Quantity as '0' for CN Legacy
            else if (deploymentProfile.Type == DeploymentProfileType.CN.ToDescription() && ApiConstants.CNNonlegacyPanOs.Any(s => s.Contains(deploymentProfile?.PanOS)))
                return 1; // Setting Firewall Quantity as '1' for CN Non Legacy
            else
                return isFromTransferDp ? deploymentProfile.FirewallsProjected : deploymentProfile.FirewallQuantity;
        }

        private int GetFirewallQuantity(Csp.Api.Model.TokenCalculatorRequest tokenCalculatorRequest)
        {
            if (tokenCalculatorRequest.Type == DeploymentProfileType.CN.ToDescription() && tokenCalculatorRequest.PanOS == ApiConstants.CNLegacyPanOs)
                return 0; // Setting Firewall Quantity as '0' for CN Legacy
            else if (tokenCalculatorRequest.Type == DeploymentProfileType.CN.ToDescription() && ApiConstants.CNNonlegacyPanOs.Any(s => s.Contains(tokenCalculatorRequest.PanOS)))
                return 1; // Setting Firewall Quantity as '1' for CN Non Legacy
            else
                return tokenCalculatorRequest.QuantityFirewall;
        }

        private string Decrypt(string encryptedText)
        {
            using (RijndaelManaged cipher = new RijndaelManaged())
            {
                cipher.Key = Encoding.UTF8.GetBytes(_aesKey256);
                cipher.IV = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                cipher.Mode = CipherMode.ECB;
                cipher.Padding = PaddingMode.PKCS7;
                string decryptedText;
                using (ICryptoTransform decryptor = cipher.CreateDecryptor())
                {
                    using (MemoryStream memoryStream = new MemoryStream(Convert.FromBase64String(encryptedText)))
                    {
                        using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader streamReader = new StreamReader(cryptoStream))
                            {
                                decryptedText = streamReader.ReadToEnd();
                            }
                        }
                    }
                }
                return decryptedText;
            }
        }

        private CreditData GetFlexData(long userAccountId, string type, string variable, string param, string paramValue, long supportAccountId = 0)
        {
            // The original query of FW Flex
            string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(_licensingUserName + ":" + _licensingPassword));
            string formattedValue = string.Format(param, paramValue);
            string graphQLRequest = string.Format(ApiConstants.TOKEN_SUMMARY_GRAPHQL, type, variable, formattedValue);
            CreditData result = TokenHelper.GetFlexData<CreditData>(_licensingApi, credentials, "POST", _tokenServiceThumbprint, graphQLRequest, userAccountId, supportAccountId);
            if (result?.Credits?.Any() ?? false)
            {
                return result;
            }

            // Additional query for CLOUD NGFW, which could not be fetched by the original query
            graphQLRequest = string.Format(ApiConstants.TOKEN_SUMMARY_FROM_ENTITLEMENT_GRAPHQL, paramValue, "ACTIVE", ApiConstants.CLOUD_NGFW_PRODUCT_ID);
            return TokenHelper.GetFlexData<CreditDataFromEntitlement>(_licensingApi, credentials, "POST", _tokenServiceThumbprint, graphQLRequest, userAccountId, supportAccountId).ToCreditDetail();
        }

        private List<CreditDataModel> GetCreditDataModel(string tokenIds, long userAccountId)
        {
            var credits = new List<CreditDataModel>();
            var responsePayload = GetFlexData(userAccountId, ApiConstants.TOKENID_TYPE, ApiConstants.TOKENID_VARIABLE, ApiConstants.TOKENID_VALUE, tokenIds);

            if (responsePayload != null && responsePayload.Credits != null)
            {
                Mapper.Map(responsePayload.Credits, credits);
            }

            var activeCreditPools = credits.Where(x => string.Compare(x.Status, CreditPoolStatus.Active.ToString(), StringComparison.OrdinalIgnoreCase) == 0).ToList();

            //activeCreditPools.ForEach(x =>
            //{
            //    if (string.Compare(x.TransactionType, "Renew", StringComparison.OrdinalIgnoreCase) != 0)
            //    {
            //        x.ExpirationDate = GetAmendRampupExpirationDate(credits);
            //    }
            //});

            var expiredCreditPools = credits.Where(x => string.Compare(x.Status, CreditPoolStatus.Expired.ToString(), StringComparison.OrdinalIgnoreCase) == 0).ToList();

            if (expiredCreditPools.Count > 0)
            {
                var activeTokenIds = activeCreditPools.Select(x => x.TokenId).ToList();
                activeCreditPools = activeCreditPools.Concat(expiredCreditPools.Where(x => !activeTokenIds.Contains(x.TokenId))).ToList();
            }

            return activeCreditPools;
        }

        private CreditDetail GetTokensConsumedDetails(long userAccountId, long supportAccountId, string graphQLRequest)
        {
            string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(_licensingUserName + ":" + _licensingPassword));
            return TokenHelper.GetFlexData<CreditDetail>(_licensingApi, credentials, "POST", _tokenServiceThumbprint, graphQLRequest, userAccountId, supportAccountId);
        }

        private CreditQuantityUsed GetCreditQuantityUsed(long userAccountId, long supportAccountId)
        {
            var graphQLRequest = string.Format(ApiConstants.TOKEN_QUANTITY_CONSUMED, supportAccountId);
            string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(_licensingUserName + ":" + _licensingPassword));
            return TokenHelper.GetFlexData<CreditQuantityUsed>(_licensingApi, credentials, "POST", _tokenServiceThumbprint, graphQLRequest, userAccountId, supportAccountId);
        }

        private Firewallsummary GetLastLoginDetails(long userAccountId, long supportAccountId, string lastLoginDate)
        {
            var graphQLRequest = string.Format(ApiConstants.LAST_LOGIN_DETAILS, supportAccountId, lastLoginDate);
            string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(_licensingUserName + ":" + _licensingPassword));
            return TokenHelper.GetFlexData<Firewallsummary>(_licensingApi, credentials, "POST", _tokenServiceThumbprint, graphQLRequest, userAccountId, supportAccountId);
        }

        private void AddLicenesAndCreditQuantityUsedToDevices(List<FlexDevice> devices, CreditQuantityUsed creditQuantityUsed, Firewallsummary firewallSummary)
        {
            var licenseDataAccess = new LicenseDataAccess();
            var licenseList = licenseDataAccess.ReadMultipleDeviceLicenses(devices.Select(x => x.DeviceId).ToArray(), false);

            devices.ForEach(x =>
            {
                if (firewallSummary != null)
                {
                    var lastLogin = firewallSummary.firewallSummary.Where(y => y.serialNumber == x.SerialNumber).FirstOrDefault();
                    if (lastLogin != null)
                    {
                        x.LastCheckinDate = lastLogin.lastLoginDate.ToString("MM/dd/yyyy");
                    }
                }

                if (creditQuantityUsed != null)
                {
                    var usageTransactionsV1 = creditQuantityUsed.usageTransactionsV1.Where(y => y.xrefCData != null && y.xrefCData.serialNumber == x.SerialNumber).FirstOrDefault();
                    if (usageTransactionsV1 != null)
                    {
                        x.CreditsUsed = usageTransactionsV1.quantityConsumed;
                    }
                    var productToken = creditQuantityUsed.usageTransactionsV1.Where(y => y.tokenId == x.TokenId).FirstOrDefault();
                    if (productToken != null)
                    {
                        x.ProductId = productToken.productId;
                        x.TokenId = productToken.tokenId;
                    }
                }

                x.Licenses = new List<SubscriptionLicense>();

                var currentLicense = licenseList.SingleOrDefault(y => y.DeviceId == x.DeviceId);
                if (currentLicense != null)
                {
                    var panVMandSupportLicences = currentLicense.Licenses.Where(y => y.LicenseName == "PA-VM" || y.LicenseNameDisplay.Contains("Support") || y.LicenseName == "Virtual Systems").ToList();
                    if (panVMandSupportLicences != null)
                    {
                        panVMandSupportLicences.ForEach(y =>
                        {
                            if (x.FirewallType == DeploymentProfileType.VM.ToDescription() && x.ProductId != ApiConstants.Subscription ||
                            (y.LicenseName != "PA-VM" && x.FirewallType == DeploymentProfileType.CN.ToDescription()))
                            {
                                x.Licenses.Add(new SubscriptionLicense
                                {
                                    Subscription = y.LicenseNameDisplay,
                                    LicenseId = y.Id,
                                    AllowDownload = y.AllowDownload
                                });
                            }
                        });
                    }
                }

                if (!string.IsNullOrEmpty(x.SubscriptionModel))
                {
                    if (x.FirewallType == DeploymentProfileType.VM.ToDescription())
                    {
                        var subsriptions = x.SubscriptionModel.Split(',');

                        for (int i = 0; i < subsriptions.Count(); i++)
                        {
                            var subscription = subsriptions[i].Split(':');
                            var subscriptionLicense = licenseList.SingleOrDefault(y => y.DeviceId == x.DeviceId);
                            var license = subscriptionLicense?.Licenses.Where(z => z.ModelName == subscription[1]).FirstOrDefault();

                            x.Licenses.Add(new SubscriptionLicense
                            {
                                Subscription = subscription[0],
                                LicenseId = license?.Id,
                                AllowDownload = license != null ? license.HasLicenseKey && !string.IsNullOrEmpty(x.ExpirationDate) && DateTime.Parse(x.ExpirationDate) >= DateTime.Now : false
                            });
                        }

                        if (x.SubscriptionModel.Contains("PAN-VM-FLEX-AURL"))
                        {
                            var subscriptionLicense = licenseList.SingleOrDefault(y => y.DeviceId == x.DeviceId);
                            var license = subscriptionLicense?.Licenses.Where(z => z.ModelName == "PAN-VM-FLEX-URL4").FirstOrDefault();
                            x.Licenses.Add(new SubscriptionLicense
                            {
                                Subscription = "URL Filtering",
                                LicenseId = license?.Id,
                                AllowDownload = license != null ? license.HasLicenseKey && !string.IsNullOrEmpty(x.ExpirationDate) && DateTime.Parse(x.ExpirationDate) >= DateTime.Now : false
                            });

                        }
                    }
                    else if (x.FirewallType == DeploymentProfileType.CN.ToDescription())
                    {
                        var lastCommaIndex = x.Subscriptions.LastIndexOf(",");
                        var license = licenseList.SingleOrDefault(y => y.DeviceId == x.DeviceId);
                        x.Licenses.Add(new SubscriptionLicense
                        {
                            Subscription = lastCommaIndex != -1 ? x.Subscriptions.Remove(lastCommaIndex, 1).Insert(lastCommaIndex, " and") : x.Subscriptions,
                            LicenseId = x.Id,
                            AllowDownload = license != null && license.Licenses.Count > 0 ? license.Licenses[0].HasLicenseKey && !string.IsNullOrEmpty(x.ExpirationDate) && DateTime.Parse(x.ExpirationDate) >= DateTime.Now : false
                        });
                    }
                }
            });

        }

        private void AddLicenesToDevices(List<PanoramaDevice> devices)
        {
            var licenseDataAccess = new LicenseDataAccess();
            var licenseList = licenseDataAccess.ReadMultipleDeviceLicenses(devices.Select(x => x.Id).ToArray(), false);

            devices.ForEach(x =>
            {
                x.Licenses = new List<SubscriptionLicense>();

                var currentLicense = licenseList.SingleOrDefault(y => y.DeviceId == x.Id);
                if (currentLicense != null)
                {
                    currentLicense.Licenses.ForEach(y =>
                    {

                        x.Licenses.Add(new SubscriptionLicense
                        {
                            Subscription = y.LicenseNameDisplay,
                            LicenseId = y.Id,
                            AllowDownload = y.AllowDownload
                        });

                    });

                }
            });

        }

        private string[] GetSubscriptions(DeploymentProfile deploymentProfile)
        {
            string[] subscriptionList = new string[] { };
            if (deploymentProfile.Subscriptions != null && deploymentProfile.Subscriptions.Any())
            {
                if (deploymentProfile.Type == DeploymentProfileType.CN.ToDescription() && deploymentProfile.PanOS == ApiConstants.CNLegacyPanOs)
                {
                    subscriptionList = GetSubscriptionsByUseCase(deploymentProfile.UseCaseId).ToArray();
                }
                else
                {
                    subscriptionList = deploymentProfile.Subscriptions;
                }
            }
            return subscriptionList;
        }

        private string GetAllocationConfiguration(DeploymentProfile deploymentProfile, bool isFromTransferDp = false)
        {
            var allocationConfigurations = new AllocationConfiguration()
            {
                QuantityFirewall = GetFirewallQuantity(deploymentProfile, isFromTransferDp),
                QuantityCPU = deploymentProfile.Type == DeploymentProfileType.CN.ToDescription() && deploymentProfile.PanOS == ApiConstants.CNLegacyPanOs ? 0 : deploymentProfile.VcpuQuantity,
                FirewallType = deploymentProfile.Type,
                QuantityVSYS = deploymentProfile.Type == DeploymentProfileType.VM.ToDescription() ? deploymentProfile.VSysQuantity : 0,
                Panorama = (deploymentProfile.Panorama != null && deploymentProfile.Panorama.Count > 0) ? deploymentProfile.Panorama.ConvertAll(delegate (string x)
                {
                    return ((PanoramaType)Enum.Parse(typeof(PanoramaType), x)).ToDescription();
                }) : deploymentProfile.Panorama,
                Subscriptions = GetSubscriptions(deploymentProfile),
                SubType = deploymentProfile.SubType,
                AvgTxtRecordsPerDay = isFromTransferDp
                    && (deploymentProfile.SubType == EDeploymentProfileSubType.API.ToString()
                    || deploymentProfile.SubType == EDeploymentProfileSubType.RT.ToString()
                    || deploymentProfile.SubType == EDeploymentProfileSubType.MS.ToString()
                    || deploymentProfile.SubType == EDeploymentProfileSubType.GW.ToString()
                    || deploymentProfile.SubType == EDeploymentProfileSubType.APIM.ToString()
                    || deploymentProfile.SubType == EDeploymentProfileSubType.AES.ToString()
                    || deploymentProfile.SubType == EDeploymentProfileSubType.CNGFW.ToString()
                    || deploymentProfile.SubType == EDeploymentProfileSubType.SK.ToString()
                    || deploymentProfile.SubType == EDeploymentProfileSubType.AF.ToString()
                    || deploymentProfile.SubType == EDeploymentProfileSubType.SPM.ToString())
                    ? deploymentProfile.AvgTxtRecordsPerDay ?? deploymentProfile.MaxThreatSimulations ?? deploymentProfile.MaxScans
                    : deploymentProfile.AvgTxtRecordsPerDay,
                KubernatesNodes = deploymentProfile.Type == DeploymentProfileType.CN.ToDescription() && deploymentProfile.PanOS == ApiConstants.CNLegacyPanOs ? deploymentProfile.FirewallQuantity : deploymentProfile.KubernatesNodes,
                MaxThreatSimulations = null, // REVERT: when AvgTxtRecordsPerDay hack is removed to - deploymentProfile.MaxThreatSimulations,
                MaxScans = null, // REVERT: when AvgTxtRecordsPerDay hack is removed to - deploymentProfile.MaxScans
                AttributesValues = deploymentProfile.AttributesValues, 
                FedRampType = deploymentProfile.FedRampType.ToString()
            };
            return JsonConvert.SerializeObject(allocationConfigurations);
        }

        private bool IsRenewalPending(DateTime poolExpirationDate, DateTime dpExpirateionDate)
        {
            return dpExpirateionDate > poolExpirationDate &&
                  dpExpirateionDate >= DateTime.Parse("2022-05-29") &&
                  dpExpirateionDate <= DateTime.Parse("2022-05-30")
                  ? true : false;

        }

        private string GetAmendRampupExpirationDate(List<CreditDataModel> credits)
        {
            var inActiveAmendAndRampup = credits.Where(x => string.Compare(x.TransactionType, "Renew", StringComparison.OrdinalIgnoreCase) != 0
                         && string.Compare(x.Status, CreditPoolStatus.Inactive.ToString(), StringComparison.OrdinalIgnoreCase) == 0).ToList();
            var expirationDate = DateTime.MinValue;
            if (inActiveAmendAndRampup != null && inActiveAmendAndRampup.Count > 0)
            {
                inActiveAmendAndRampup.ForEach(x =>
                {
                    if (expirationDate < DateTime.Parse(x.ExpirationDate))
                    {
                        expirationDate = DateTime.Parse(x.ExpirationDate);
                    }
                });
            }
            else
            {
                var activeAmendAndRampup = credits.Where(x => string.Compare(x.TransactionType, "Renew", StringComparison.OrdinalIgnoreCase) != 0
                         && string.Compare(x.Status, CreditPoolStatus.Active.ToString(), StringComparison.OrdinalIgnoreCase) == 0).FirstOrDefault();
                if (activeAmendAndRampup != null)
                {
                    expirationDate = DateTime.Parse(activeAmendAndRampup.ExpirationDate);
                }
                else
                {
                    var expiredAmendAndRampup = credits.Where(x => string.Compare(x.TransactionType, "Renew", StringComparison.OrdinalIgnoreCase) != 0
                         && string.Compare(x.Status, CreditPoolStatus.Expired.ToString(), StringComparison.OrdinalIgnoreCase) == 0).ToList();

                    if (expiredAmendAndRampup != null && expiredAmendAndRampup.Count > 0)
                    {
                        expiredAmendAndRampup.ForEach(x =>
                        {
                            if (expirationDate < DateTime.Parse(x.ExpirationDate))
                            {
                                expirationDate = DateTime.Parse(x.ExpirationDate);
                            }
                        });
                    }
                }
            }

            return expirationDate.ToString("MM/dd/yyyy");

        }

        private List<PoolMessage> GetCreditPoolMessages(List<CreditDataModel> credits)
        {
            var PoolMessages = new List<PoolMessage>();

            var activeAmend = credits.Where(x => string.Compare(x.TransactionType, "Amend", StringComparison.OrdinalIgnoreCase) == 0
                     && string.Compare(x.Status, CreditPoolStatus.Active.ToString(), StringComparison.OrdinalIgnoreCase) == 0).FirstOrDefault();

            if (activeAmend != null)
            {
                PoolMessages.Add(new PoolMessage { Type = "AmendAndExtend", Message = string.Format("Amend: {0}", activeAmend.EffectiveDate) });
            }

            var filteredPool = credits.Where(x => string.Compare(x.Status, CreditPoolStatus.Expired.ToString(), StringComparison.OrdinalIgnoreCase) != 0)
                                      .OrderBy(y => DateTime.Parse(y.EffectiveDate)).ToList();

            if (filteredPool.Count > 1)
            {
                var rampUpDataList = new List<RampUpData>();
                int rowId = 0;
                filteredPool.ForEach(x =>
                {
                    rowId++;
                    var rampUpData = new RampUpData
                    {
                        RampId = rowId,
                        TransactionType = x.TransactionType == "BUY" ? "New" : CultureInfo.CurrentCulture.TextInfo.ToTitleCase(x.TransactionType.ToLower()),
                        StartDate = DateTime.Parse(x.EffectiveDate).ToString("MM/dd/yyy"),
                        EndDate = DateTime.Parse(x.ExpirationDate).ToString("MM/dd/yyyy"),
                        Credits = x.TotalPurchased,
                        Status = x.Status
                    };
                    rampUpDataList.Add(rampUpData);
                });
                PoolMessages.Add(new PoolMessage { Type = "RampUp", Data = rampUpDataList });
            }
            return PoolMessages;
        }

        public IEnumerable<DeploymentProfileForHub> GetDeploymentProfiles(long supportAccountId)
        {
           return _flexDataAccess.GetDeploymentProfiles(supportAccountId);
        }


        private Func<string, string, DeploymentProfileAttributeValue[]> GetZeroValueAttribute = (t, s) =>
        {
            return t == DeploymentProfileType.AI.ToString() && s == EDeploymentProfileSubType.API.ToString()
                ? new DeploymentProfileAttributeValue[] { new DeploymentProfileAttributeValue { UnitOfMeasure = FLEX_ATTRIBUTE_NAME_BT, Value = "0" } }
                : t == DeploymentProfileType.AI.ToString() && s == EDeploymentProfileSubType.RT.ToString()
                ? new DeploymentProfileAttributeValue[] { new DeploymentProfileAttributeValue { UnitOfMeasure = FLEX_ATTRIBUTE_NAME_RT, Value = "0" } }
                : t == DeploymentProfileType.AI.ToString() && s == EDeploymentProfileSubType.MS.ToString()
                ? new DeploymentProfileAttributeValue[] { new DeploymentProfileAttributeValue { UnitOfMeasure = FLEX_ATTRIBUTE_NAME_MS, Value = "false" } }
                : t == DeploymentProfileType.AI.ToString() && s == EDeploymentProfileSubType.GW.ToString()
                ? new DeploymentProfileAttributeValue[] { 
                    new DeploymentProfileAttributeValue { UnitOfMeasure = FLEX_ATTRIBUTE_NAME_GW_DEPLOYMENT, Value = "" }, 
                    new DeploymentProfileAttributeValue { UnitOfMeasure = FLEX_ATTRIBUTE_NAME_GW_GW_GATEWAY, Value = "0" }, 
                    new DeploymentProfileAttributeValue { UnitOfMeasure = FLEX_ATTRIBUTE_NAME_GW_REQUESTS, Value = "0" }
                }
                : t == DeploymentProfileType.AI.ToString() && s == EDeploymentProfileSubType.APIM.ToString()
                ? new DeploymentProfileAttributeValue[] { new DeploymentProfileAttributeValue { UnitOfMeasure = FLEX_ATTRIBUTE_NAME_APIM, Value = "0" } }
                : t == DeploymentProfileType.AI.ToString() && s == EDeploymentProfileSubType.AES.ToString()
                ? new DeploymentProfileAttributeValue[] { new DeploymentProfileAttributeValue { UnitOfMeasure = FLEX_ATTRIBUTE_NAME_AES, Value = "0" } }
                : t == DeploymentProfileType.CLOUD.ToString() && s == EDeploymentProfileSubType.CNGFW.ToString()
                ? new DeploymentProfileAttributeValue[] { new DeploymentProfileAttributeValue { UnitOfMeasure = FLEX_ATTRIBUTE_NAME_CLOUD_CNGFW, Value = "0" } }
                : t == DeploymentProfileType.AI.ToString() && s == EDeploymentProfileSubType.SK.ToString()
                ? new DeploymentProfileAttributeValue[0]
                : t == DeploymentProfileType.AI.ToString() && s == EDeploymentProfileSubType.AF.ToString()
                ? new DeploymentProfileAttributeValue[0]
                : t == DeploymentProfileType.AI.ToString() && s == EDeploymentProfileSubType.SPM.ToString()
                ? new DeploymentProfileAttributeValue[] { new DeploymentProfileAttributeValue { UnitOfMeasure = FLEX_ATTRIBUTE_NAME_SPM, Value = "0" } }
                : new DeploymentProfileAttributeValue[0];
        };

        #endregion private method
    }
}