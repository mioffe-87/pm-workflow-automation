using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.Flex
{
    public class DeploymentProfileRequest
    {
        public int SupportAccountId { get; set; }
        public string Authcode { get; set; }
        public int VcpuQuantity { get; set; }
        public string Subscriptions { get; set; }
        public DateTime ExpirationDate { get; set; }
        public string TokenId { get; set; }

        public string Entitlements { get; set; }
        public string PartId { get; set; }
        public bool IsLegacy { get; set; }
        public int NumberOfFirewalls { get; set; }
        public string VmType { get; set; }
        public string SupportType { get; set; }
        public long AscId { get; set; }
        public string AscName { get; set; }
        public int KubernatesNodes { get; set; }
        public CnTokens Tokens { get; set; }
        public bool IsSubscriptionOnly { get; set; }
        public bool IsAirgapEligible { get; set; }
    }

    public class CnxDeploymentProfileRequest
    {

        [JsonProperty(PropertyName = "dpAuthcode", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string DpAuthcode { get; set; }

        [JsonProperty(PropertyName = "supportAccountId", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int SupportAccountId { get; set; }

        [JsonProperty(PropertyName = "tokens", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Tokens { get; set; }

        [JsonProperty(PropertyName = "entitlements", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Entitlements { get; set; }

        [JsonProperty(PropertyName = "expiration", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DateTime Expiration { get; set; }

        [JsonProperty(PropertyName = "tokenId", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string TokenId { get; set; }

        [JsonProperty(PropertyName = "partId", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string PartId { get; set; }
     
        [JsonProperty(PropertyName = "ascId", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public long AscId { get; set; }

        [JsonProperty(PropertyName = "ascName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string AscName { get; set; }

        [JsonProperty(PropertyName = "supportType", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string SupportType { get; set; }
    }
}
