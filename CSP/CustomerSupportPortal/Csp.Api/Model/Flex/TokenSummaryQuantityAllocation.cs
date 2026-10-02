using Newtonsoft.Json;
using System.Collections.Generic;

namespace Csp.Api.Model.Flex
{
    public class TokenSummaryQuantityAllocation
    {
        [JsonProperty(PropertyName = "tokenId")]
        public long TokenId { get; set; }

        [JsonProperty(PropertyName = "supportAccountId")]
        public long SupportAccountId { get; set; }

        [JsonProperty(PropertyName = "quantityAllocation")]
        public List<QuantityAllocation> QuantityAllocation { get; set; }
    }
}