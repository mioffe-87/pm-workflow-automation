using Csp.Api.Model.Enums;
using Csp.Api.Model.Flex;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api
{
    public class FirewallFlexDeploymentProfile
    {
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "subType")]
        public string SubType { get; set; }

        [JsonProperty(PropertyName = "vm_model")]
        public string VmModel { get; set; }

        [JsonProperty(PropertyName = "support_account_id")]
        public long SupportAccountId { get; set; }

        [JsonProperty(PropertyName = "source_token_id")]
        public long SourceTokenId { get; set; }

        [JsonProperty(PropertyName = "token_id")]
        public long TokenId { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string ProfileName { get; set; }

        [JsonProperty(PropertyName = "v_cpu_quantity")]
        public int VcpuQuantity { get; set; }

        [JsonProperty(PropertyName = "use_cases")]
        public List<int> UseCaseId { get; set; }

        [JsonProperty(PropertyName = "subscriptions")]
        public List<string> Subscriptions { get; set; }

        [JsonProperty(PropertyName = "panorama")]
        public List<string> Panorama { get; set; }

        [JsonProperty(PropertyName = "pan_OS")]
        public string PanOS { get; set; }

        [JsonProperty(PropertyName = "user_account_id")]
        public long UserAccountId { get; set; }
        
        [JsonProperty(PropertyName = "is_air_gap_enabled")]
        public bool IsAirGapEnabled { get; set; }

        [JsonProperty(PropertyName = "v_sys_quantity")]
        public int VSysQuantity { get; set; }

        [JsonProperty(PropertyName = "is_renewal")]
        public bool IsRenewal { get; set; }

        [JsonProperty(PropertyName = "features")]
        public List<string> Features { get; set; }

        [JsonProperty(PropertyName = "firewall_quantity")]
        public int FirewallQuantity { get; set; }

        [JsonProperty(PropertyName = "auth_code")]
        public string AuthCode { get; set; }

        [JsonProperty(PropertyName = "avgTxtRecordsPerDay")]
        public int? AvgTxtRecordsPerDay { get; set; }

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
    }

    public class TokenCalculatorRequest
    {

        [JsonProperty("token_id")]
        public long TokenId { get; set; }

        [JsonProperty("product_id")]
        public string ProductId { get; set; }

        [JsonProperty("support_type")]
        public string SupportType { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "subType")]
        public string SubType { get; set; }

        [JsonProperty("quantity_firewall")]
        public int QuantityFirewall { get; set; }

        [JsonProperty("quantity_cpu")]
        public int NumberOfCPU { get; set; }

        [JsonProperty("panorama")]
        public List<string> Panorama { get; set; }

        [JsonProperty("subscriptions")]
        public List<string> Subscriptions { get; set; }
        
        [JsonProperty("quantity_node")]
        public int KubernatesNodes { get; set; }

        [JsonProperty("pan_os", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string PanOS { get; set; }

        [JsonProperty("quantity_vsys", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int NumberOfvSYS { get; set; }

        [JsonProperty(PropertyName = "avgTxtRecordsPerDay")]
        public int? AvgTxtRecordsPerDay { get; set; }

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
