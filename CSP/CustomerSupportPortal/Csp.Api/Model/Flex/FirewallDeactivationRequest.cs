
using Csp.Api.Model.Enums;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Csp.Api.Model.Flex
{

    public class FirewallDeactivationRequest
    {
        [JsonProperty("userAccountId")]
        public long UserAccountId { get; set; }

        [JsonProperty("supportAccountId")]
        public long SupportAccountId { get; set; }

        [JsonProperty("serialNumbers")]
        public List<string> SerialNumber { get; set; }

        [JsonProperty(PropertyName = "tokenId")]
        public long TokenId { get; set; }

        [JsonProperty(PropertyName = "productId")]
        public string ProductId { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "subType")]
        public string SubType { get; set; }

        [JsonProperty(PropertyName = "authCode")]
        public string AuthCode { get; set; }

        [JsonProperty(PropertyName = "panOs")]
        public string PanOs { get; set; }

        [JsonProperty(PropertyName = "fed_ramp_type")]
        public EFedRampType FedRampType { get; set; }
    }
}
