using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.ExtendedExpertise
{
    public class ExtendedExpertiseUsageTransactionXrefCData
    {
        [JsonProperty("transactionType")]
        public string TransactionType { get; set; } // Renewal

        [JsonProperty("pricePerMonth")]
        public int? PricePerMonth { get; set; }

        [JsonProperty("revShare")]
        public double? RevShare { get; set; }

        [JsonProperty("vendorId")]
        public string VendorId { get; set; }

        [JsonProperty("CSPID")]
        public long? CspId { get; set; }

        [JsonProperty("vendorName")]
        public string VendorName { get; set; }

        [JsonProperty("debt")]
        public double? debt { get; set; }

        [JsonProperty("userDetails")]
        public ExtendedExpertiseUsageTransactionXrefCDataUserDetails UserDetails { get; set; }

        [JsonProperty("expertiseDetails")]
        public ExtendedExpertiseUsageTransactionXrefCDataExpertiseDetails ExpertiseDetails { get; set; }

        [JsonProperty("sapDetails")]
        public ExtendedExpertiseUsageTransactionXrefCDataSapDetails SapDetails { get; set; }

        [JsonProperty("customerName")]
        public string CustomerName { get; set; }
    }
}
