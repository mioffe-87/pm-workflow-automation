using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.ExtendedExpertise
{
    public class ExtendedExpertiseAsset
    {
        [JsonProperty("salesAccountId")]
        public string SalesAccountId { get; set; }

        [JsonProperty("salesAccountName")]
        public string SalesAccountName { get; set; }

        [JsonProperty("unitOfMeasure")]
        public string UnitOfMeasure { get; set; }

        [JsonProperty("quantityAllocated")]
        public double? QuantityAllocated { get; set; }

        [JsonProperty("quantityNetBalance")]
        public double? QuantityNetBalance { get; set; }

        [JsonProperty("quantityNetConsumption")]
        public double? QuantityNetConsumption { get; set; }
    }
}
