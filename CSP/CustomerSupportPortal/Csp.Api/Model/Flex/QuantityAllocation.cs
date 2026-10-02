using Newtonsoft.Json;

namespace Csp.Api.Model.Flex
{
    public class QuantityAllocation
    {
        [JsonProperty(PropertyName = "allocationId")]
        public string AllocationId { get; set; }

        [JsonProperty(PropertyName = "quantityAllocated")]
        public double QuantityAllocated { get; set; }

        [JsonProperty(PropertyName = "quantityConsumed")]
        public double QuantityConsumed { get; set; }

        [JsonProperty(PropertyName = "consumptionRenewalStatus")]
        public string ConsumptionRenewalStatus { get; set; }
        
    }
}
