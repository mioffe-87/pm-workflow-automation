using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.ExtendedExpertise
{
    public class ExtendedExpertiseUsageTransaction
    {
        [JsonProperty("productId")]
        public string ProductId { get; set; } // "41e65c09-a6cf-4509-b44e-7c0d90d0abb8"

        [JsonProperty("unitOfMeasure")]
        public string UnitOfMeasure { get; set; }

        [JsonProperty("companyCode")]
        public string CompanyCode { get; set; }

        [JsonProperty("consumptionDate")]
        public string ConsumptionDate { get; set; } // "2020-12-06T19:47:39Z"

        [JsonProperty("quantityConsumed")]
        public double? QuantityConsumed { get; set; }

        [JsonProperty("usageDescription")]
        public string UsageDescription { get; set; }

        [JsonProperty("xrefPackId")]
        public string XrefPackId { get; set; }

        [JsonProperty("xrefTransactionId")]
        public string XrefTransactionId { get; set; } // "35a92335-2373-40ea-83fb-ceb82031bef1"

        [JsonProperty("xrefCData")]
        public ExtendedExpertiseUsageTransactionXrefCData XrefCData { get; set; }

    }
}
