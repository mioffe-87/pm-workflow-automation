using Csp.Api.Model.Enums;
using Csp.Api.Model.Flex;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Csp.Api.Model
{
    public class TokenCalculatorRequest
    {

        [JsonProperty("tokenId", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public long TokenId { get; set; }

        [JsonProperty("productId")]
        public string ProductId { get; set; }

        [JsonProperty("supportType", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string SupportType { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("subType")]
        public string SubType { get; set; }

        [JsonProperty("quantityFirewall")]
        public int QuantityFirewall { get; set; }

        [JsonProperty("quantityCPU", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int NumberOfCPU { get; set; }

        [JsonProperty("panorama", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<string> Panorama { get; set; }

        [JsonProperty("subscriptions", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<string> Subscriptions { get; set; }

        //[JsonIgnore]
        [JsonProperty("quantityNode")]
        public int KubernatesNodes { get; set; }

        [JsonProperty("panOs", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string PanOS { get; set; }

        [JsonProperty("quantityVSYS", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int NumberOfvSYS { get; set; }

        [JsonProperty("avgTxtRecordsPerDay")]
        public int AvgTxtRecordsPerDay { get; set; }

        [JsonProperty(PropertyName = "max_threat_simulations")]
        public int? MaxThreatSimulations { get; set; }

        [JsonProperty(PropertyName = "max_scans")]
        public int? MaxScans { get; set; }

        [JsonProperty(PropertyName = "fed_ramp_type")]
        public EFedRampType FedRampType { get; set; }

        [JsonProperty(PropertyName = "attributes")]
        public DeploymentProfileAttributeValue[] AttributesValues { get; set; }
    }
}