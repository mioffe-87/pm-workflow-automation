using Csp.Api.Model.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.Flex
{
    public class FlexDevice
    {
        public long Id { get; set; }
        public long SupportAccountId { get; set; }
        public string FirewallType { get; set; }

        public string SerialNumber { get; set; }

        public string DeviceName { get; set; }

        public long? VCpuQuantity { get; set; }

        public string VMModel { get; set; }

        public string Subscriptions { get; set; }

        public string AuthorizationCode { get; set; }

        public string ExpirationDate { get; set; }

        public float CreditsUsed { get; set; }

        public string AscName { get; set; }

        public long DeviceId { get; set; }

        [JsonIgnore]
        public string SubscriptionModel { get; set; }

        public List<SubscriptionLicense> Licenses { get; set; }

        public string PanOs { get; set; }

        public string ProductId { get; set; }

        public string LastCheckinDate { get; set; }

        public long TokenId { get; set; }

        public int IsSubOnly { get; set; }

        public bool IsWarmFw { get; set; }

        public EFedRampType FedRampType { get; set; }

        public DeploymentProfileAttributeValue[] Attributes { get; set; } = Array.Empty<DeploymentProfileAttributeValue>();
    }

    public class SubscriptionLicense
    {
        public long? LicenseId { get; set; }

        public string Subscription { get; set; }

        public bool AllowDownload { get; set; }
    }

    public class FlexDeviceRequest
    {
        public RequestParameters requestParameters { get; set; }
    }

    public class RequestParameters
    {
        [JsonProperty(PropertyName = "page")]
        public int PaginationOffset { get; set; }

        [JsonProperty(PropertyName = "size")]
        public int PaginationCount { get; set; }

        [JsonProperty(PropertyName = "sort")]
        public string Sort { get; set; }

        [JsonProperty(PropertyName = "search_field")]
        public string SearchField { get; set; }

        [JsonProperty(PropertyName = "search_value")]
        public string SearchValue { get; set; }
    }

    public class FlexDevices
    {
        [JsonProperty(PropertyName = "data")]
        public FlexDevicesList FlexDeviceList { get; set; }
    }

    public class FlexDevicesList
    {
        public List<FlexDevice> devices { get; set; }
        public long totalElementsCount { get; set; }
        public long totalPages { get; set; }
    }

    public class PanoramaDevice
    {
        public long Id { get; set; }
        public string SerialNumber { get; set; }
        public string PanoramaModel { get; set; }
        public string AuthorizationCode { get; set; }
        public string ExpirationDate { get; set; }
        public string DeploymentProfile { get; set; }
        public long CreditPoolId { get; set; }
        public List<SubscriptionLicense> Licenses { get; set; }
        public bool IsFwFlexPanorama { get; set; }

        public EFedRampType FedRampType { get; set; }
    }

    public class PanoromaDeviceList
    {
        public List<PanoramaDevice> panoramas { get; set; }
        public long totalElementsCount { get; set; }
        public long totalPages { get; set; }
    }

    public class PanoramaDevices
    {
        [JsonProperty(PropertyName = "data")]
        public PanoromaDeviceList PanoromaDevices { get; set; }
    }


    public class CnFirewalls  {

        public List<CnFirewall> devices { get; set; }
        public long totalElementsCount { get; set; }
        public long totalPages { get; set; }
    }

    

    public class CnFirewall
    {
        public int Id { get; set; }      
        public long SupportAccountId { get; set; }        
        public string FirewallType { get; set; }  
        public string SerialNumber { get; set; }      
        public string DeviceName { get; set; }     
        public string Subscriptions { get; set; }
        public string AuthorizationCode { get; set; }
        public string ExpirationDate { get; set; }     
        public float CreditsUsed { get; set; }
        public string AscName { get; set; }
        public long DeviceId { get; set; }
        public List<SubscriptionLicense> Licenses { get; set; }
        public string PanOs { get; set; }     
        public string ProductId { get; set; }
        public int IsSubOnly { get; set; }
    }

    public class FileInfo
    {
        public string FileName { get; set; }
        public MemoryStream Content { get; set; }
    }

    public class RetryFailedDPRequest
    {

        [JsonProperty(PropertyName = "id")]
        public long Id { get; set; }

        [JsonProperty("supportAccountId")]
        public long SupportAccountId { get; set; }

        [JsonProperty("userAccountId")]
        public long UserAccountId { get; set; }
    }

}
