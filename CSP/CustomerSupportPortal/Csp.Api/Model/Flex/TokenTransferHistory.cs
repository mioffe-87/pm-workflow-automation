using Newtonsoft.Json;
namespace Csp.Api.Model
{
    public class TokenTransferHistory
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("dateTime")]
        public string RequestedOn { get; set; }

        [JsonProperty("modifiedBy")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("quantity")]
        public double Quantity { get; set; }

        [JsonProperty("transactionType")]
        public string TransactionType { get; set; }

        [JsonProperty("endBalance")]
        public double EndBalance { get; set; }
    }
}
