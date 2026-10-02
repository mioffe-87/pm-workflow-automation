using Csp.Api.Model.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.Flex
{
    public class AllocateToken
    {
        [JsonProperty("source_token_id")]
        public long SourceTokenId { get; set; }

        [JsonProperty("token_id")]
        public long TokenId { get; set; }

        [JsonProperty("allocation_id")]
        public string Authcode { get; set; }

        [JsonProperty("product_id")]
        public string ProductId { get; set; }

        [JsonProperty("type")]
        public string FirewallType { get; set; }

        [JsonProperty("subType")]
        public string FirewallSubType { get; set; }

        [JsonProperty("quantity")]
        public int Quantity { get; set; }

        [JsonProperty("config")]
        public string AllocationConfigurations { get; set; }

        [JsonProperty("usage_description")]
        public string UsageDescription { get; set; }

        [JsonProperty("xref_transaction_id")]
        public string ReferenceTransactionId { get; set; }

        [JsonProperty("xref_c_data")]
        public string ReferenceCustomData { get; set; }

        [JsonProperty("transaction_date")]
        [JsonConverter(typeof(FwFlexDateFormatConverter), "yyyy-MM-ddThh:mm:ss.fffZ")]
        public DateTime TransactionDate { get; set; }
                
        [JsonProperty("source_allocation_id")]
        public string SourceAuthcode { get; set; }

        [JsonProperty("support_account_id")]
        public long SupportAccountId { get; set; }

        [JsonProperty("target_token_id")]
        public long TargetTokenId { get; set; }

        [JsonProperty("update_consumed_records")]
        public bool UpdateConsumedRecords { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty(PropertyName = "fed_ramp_type")]
        public string FedRampType { get; set; }

        [JsonProperty(PropertyName = "attributes")]
        public DeploymentProfileAttributeValue[] AttributesValues { get; set; }
    }

    public class AllocationConfiguration
    {
        [JsonProperty("type")]
        public string FirewallType { get; set; }

        [JsonProperty("quantityFirewall")]
        public int QuantityFirewall { get; set; }

        [JsonProperty("quantityCPU")]
        public int QuantityCPU { get; set; }

        [JsonProperty("panorama")]
        public List<string> Panorama { get; set; }

        [JsonProperty("subscriptions")]
        public string[] Subscriptions { get; set; }

        [JsonProperty(PropertyName = "quantityNode")]
        public int KubernatesNodes { get; set; }

        [JsonProperty("quantityVSYS")]
        public int QuantityVSYS { get; set; }

        [JsonProperty("avgTxtRecordsPerDay")]
        public int? AvgTxtRecordsPerDay { get; set; }

        [JsonProperty("subType")]
        public string SubType { get; set; }
        [JsonProperty(PropertyName = "max_threat_simulations")]
        public int? MaxThreatSimulations { get; set; }

        [JsonProperty(PropertyName = "max_scans")]
        public int? MaxScans { get; set; }

        [JsonProperty(PropertyName = "fed_ramp_type")]
        public string FedRampType { get; set; }

        [JsonProperty(PropertyName = "attributes")]
        public DeploymentProfileAttributeValue[] AttributesValues { get; set; }
    }

    public class ReferenceCustomData
    {
        [JsonProperty("Field1")]
        public string Field1 { get; set; }
    }
}
