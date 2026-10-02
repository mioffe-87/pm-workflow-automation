using Csp.Api.Model.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.Flex
{
    public class DeploymentProfiles
    {
        public IList<DeploymentProfileData> data { get; set; }
    }

    public class DeploymentProfileDto
    {
        public DeploymentProfileData data { get; set; }
    }

    public class DeploymentProfileData
    {

        [JsonProperty(PropertyName = "profileName")]
        public string ProfileName { get; set; }

        [JsonProperty(PropertyName = "dAuthCode")]
        public string AuthCode { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "panOsVersion")]
        public string PanOS { get; set; }

        [JsonProperty(PropertyName = "creditsAllocated")]
        public double TokensAllocated { get; set; }

        [JsonProperty(PropertyName = "creditsConsumed")]
        public double TokensConsumed { get; set; }

        [JsonProperty(PropertyName = "vCpuConsumed")]
        public int VcpuConsumed { get; set; }

        [JsonProperty(PropertyName = "vCpuAllocated")]
        public int VcpuAllocated { get; set; }

        [JsonProperty(PropertyName = "fWsDeployed")]
        public int FirewallsCreated { get; set; }

        [JsonProperty(PropertyName = "fWsPlanned")]
        public int FirewallsProjected { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }
    }

    public class DeploymentProfileList
    {
        public List<DeploymentProfile> profiles { get; set; }
        public Dictionary<string, string> availableSubscriptions { get; set; }
    }

    public class DeploymentProfile
    {
        [JsonProperty(PropertyName = "id")]
        public long? Id { get; set; }

        [JsonProperty(PropertyName = "tokensConsumed")]
        public double TokensConsumed { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "subType")]
        public string SubType { get; set; }

        [JsonProperty(PropertyName = "subTypeAltName")]
        public string SubTypeAltName { get; set; }

        [JsonProperty(PropertyName = "flattenedType")]
        public string FlattenedName { get; set; }

        [JsonProperty(PropertyName = "subTypeDisplayName")]
        public string SubTypeDisplayName { get; set; }

        [JsonProperty(PropertyName = "subTypeDescription")]
        public string SubTypeDescription { get; set; }

        [JsonProperty(PropertyName = "firewallsCreated")]
        public int FirewallsCreated { get; set; }

        [JsonProperty(PropertyName = "firewallsProjected")]
        public int FirewallsProjected { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "vmModel")]
        public string VmModel { get; set; }

        [JsonProperty(PropertyName = "supportAccountId", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public long SupportAccountId { get; set; }

        [JsonProperty(PropertyName = "sourceTokenId", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public long SourceTokenId { get; set; }

        [JsonProperty(PropertyName = "tokenId", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public long TokenId { get; set; }

        [JsonProperty(PropertyName = "productId", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ProductId { get; set; }

        [JsonProperty(PropertyName = "authCode")]
        public string AuthCode { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string ProfileName { get; set; }

        [JsonProperty(PropertyName = "firewallQuantity", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int FirewallQuantity { get; set; }

        [JsonProperty(PropertyName = "vCpuQuantity", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int VcpuQuantity { get; set; }

        [JsonProperty(PropertyName = "tokensAllocated")]
        public double TokensAllocated { get; set; }

        [JsonProperty(PropertyName = "useCase")]
        public List<int> UseCaseId { get; set; }
        [JsonProperty(PropertyName = "subscriptions", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string[] Subscriptions { get; set; }

        [JsonProperty(PropertyName = "features", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string[] Features { get; set; }

        [JsonProperty(PropertyName = "panorama")]
        public List<string> Panorama { get; set; }

        [JsonProperty(PropertyName = "panOS")]
        public string PanOS { get; set; }

        [JsonProperty(PropertyName = "expirationDate", DefaultValueHandling = DefaultValueHandling.Ignore)]      
        public DateTime ExpirationDate { get; set; }

        [JsonProperty(PropertyName = "createdBy", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public long CreatedBy { get; set; }

        [JsonProperty(PropertyName = "securityFeatures", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string SecurityFeatures { get; set; }

        [JsonProperty(PropertyName = "sku", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Sku { get; set; }

        [JsonProperty(PropertyName = "supportType", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string SupportType { get; set; }

        [JsonProperty(PropertyName = "vcpuConsumed")]
        public int VcpuConsumed { get; set; }

        [JsonProperty(PropertyName = "vcpuAvailable", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int VcpuAvailable { get; set; }

        [JsonProperty(PropertyName = "vcpuAllocated")]
        public int VcpuAllocated { get; set; }

        [JsonProperty(PropertyName = "serialNumber", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Dictionary<string, string> ReferenceCustomData { get; set; }

        [JsonIgnore]
        [JsonProperty(PropertyName = "quantityNode")]
        public int KubernatesNodes { get; set; }

        [JsonProperty(PropertyName = "hasPanorama")]
        public bool HasPanorama { get; set; }

        [JsonProperty(PropertyName = "creditPoolRenewalStatus")]
        public string CreditPoolRenewalStatus { get; set; }

        [JsonProperty(PropertyName = "consumptionRenewalStatus")]
        public string ConsumptionRenewalStatus { get; set; }

        [JsonProperty(PropertyName = "isRenewalPending")]
        public bool IsRenewalPending { get; set; }

        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }

        [JsonProperty(PropertyName = "userAccountId")]
        public long UserAccountId { get; set; }

        [JsonProperty(PropertyName = "isHubSetupRequired")]
        public bool IsHubSetupRequired { get; set; }

        [JsonProperty(PropertyName = "sourceAuthCode")]
        public string SourceAuthcode { get; set; }
        [JsonProperty(PropertyName = "genType")]
        public string GenType { get; set; }

        [JsonProperty(PropertyName = "isAirgapEligible")]
        public bool IsAirgapEligible { get; set; }

        [JsonProperty(PropertyName = "vSysQuantity")]
        public int VSysQuantity { get; set; }

        [JsonProperty(PropertyName = "isRenewal")]
        public bool isRenewal { get; set; }

        [JsonProperty(PropertyName = "avgTxtRecordsPerDay")]
        public int? AvgTxtRecordsPerDay { get; set; }

        [JsonProperty(PropertyName = "isAiopsMigrated")]
        public bool IsAiopsMigrated { get; set; }

        [JsonProperty(PropertyName = "state", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string State { get; set; }

        [JsonProperty(PropertyName = "max_threat_simulations")]
        public int? MaxThreatSimulations { get; set; }

        [JsonProperty(PropertyName = "max_scans")]
        public int? MaxScans { get; set; }

        [JsonProperty(PropertyName = "note")]
        public string Note { get; set; }

        [JsonProperty(PropertyName = "fed_ramp_type")]
        public EFedRampType FedRampType { get; set; }

        [JsonProperty(PropertyName = "attributes")]
        public DeploymentProfileAttributeValue[] AttributesValues { get; set; }

        [JsonProperty(PropertyName = "isgraceallowed")]
        public bool IsGraceAllowed { get; set; }
    }

    public class DeploymentProfileInfo
    {
        public long SupportAccountId { get; set; }
        public long TokenId { get; set; }
        public int NumberOfDeployments { get; set; }
    }

    public class TokenAllocationDetail
    {
        public double NetBalance { get; set; }
        public double TokensAllocated { get; set; }
        public double TotalAvailableTokens { get; set; }
    }

    public class CnTokens
    {
        public int node_tokens { get; set; }
        public int core_tokens { get; set; }
    }

    public class DeleteDeploymentProfileRequest
    {

        [JsonProperty(PropertyName = "supportAccountId")]
        public long SupportAccountId { get; set; }

        [JsonProperty(PropertyName = "userAccountId")]
        public long UserAccountId { get; set; }

        [JsonProperty(PropertyName = "productId")]
        public string ProductId { get; set; }

        [JsonProperty(PropertyName = "tokenId")]
        public long TokenId { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "authCode")]
        public string AuthCode { get; set; }

        [JsonProperty(PropertyName = "pan_OS")]
        public string PanOS { get; set; }

        [JsonProperty(PropertyName = "subType")]
        public string SubType { get; set; }

    }

    public class DeploymentProfileStatus
    {
        [JsonProperty(PropertyName = "profileId")]
        public long Id { get; set; }

        [JsonProperty(PropertyName = "authCode")]
        public string AuthCode { get; set; }

        [JsonProperty(PropertyName = "success")]
        public bool Success { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }
    }

    public class DeploymentProfileForHub
    {
        [JsonProperty(PropertyName = "deploymentProfileId")]
        public long DeploymentProfileId { get; set; }

        [JsonProperty(PropertyName = "profileName")]
        public string ProfileName { get; set; }

        [JsonProperty(PropertyName = "dAuthCode")]
        public string AuthCode { get; set; }

        [JsonProperty(PropertyName = "serialNumbers")]
        public List<string> SerialNumber { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "subscriptions")]
        public List<HubSubscription> Subscriptions { get; set; }
    }

    public class DeploymentProfileHubDto
    {
        public IEnumerable<DeploymentProfileForHub> data;
    }


    public class DeploymentProfileHubStatus
    {
        [JsonProperty(PropertyName = "dAuthCode")]
        public string AuthCode { get; set; }

        [JsonProperty(PropertyName = "subscriptions")]
        public List<SubscriptionStatus> Subscriptions { get; set; }
    }

    public class SubscriptionStatus
    {
        [JsonProperty(PropertyName = "subscription")]
        public string Subscription { get; set; }

        [JsonProperty(PropertyName = "status")]
        public int Status { get; set; }
    }

    public class HubSubscription
    {
        [JsonProperty(PropertyName = "subscription")]
        public string Subscription { get; set; }

        [JsonProperty(PropertyName = "app_id")]
        public string AppId { get; set; }

        [JsonProperty(PropertyName = "package_version", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int PackageVersion { get; set; }

        [JsonProperty(PropertyName = "status")]
        public int Status { get; set; }
    }
}
