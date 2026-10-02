using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.Flex
{
    public class CreditTransferRequest
    {
        [JsonIgnore]
        public long UserAccountId { get; set; }

        [JsonIgnore]
        public long SourceSupportAccountId { get; set; }

        [JsonProperty(PropertyName = "targetSupportAccountId")]
        public long? DestinationSupportAccountId { get; set; }

        [JsonProperty(PropertyName = "tokenId")]
        public long? SourceTokenId { get; set; }

        [JsonProperty(PropertyName = "targetTokenId")]
        public long? DestinationTokenId { get; set; }

        [JsonProperty(PropertyName = "productId")]
        public string SourceProductId { get; set; }

        [JsonProperty(PropertyName = "targetProductId")]
        public string DestinationProductId { get; set; }

        [JsonProperty(PropertyName = "sku")]
        public string Sku { get; set; }

        [JsonProperty(PropertyName = "transactionDate")]
        [JsonConverter(typeof(FwFlexDateFormatConverter), "yyyy-MM-ddThh:mm:ss.fffZ")]
        public DateTime TransactionDate { get; set; }

        [JsonProperty(PropertyName = "xrefTransactionId")]
        public string CrossRefTransactionId { get; set; }

        [JsonProperty(PropertyName = "quantity")]
        public double Quantity { get; set; }

        [JsonProperty(PropertyName = "usageDescription")]
        public string UsageDescription { get; set; }
    }
}
