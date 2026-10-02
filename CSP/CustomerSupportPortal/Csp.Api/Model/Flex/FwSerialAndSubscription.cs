
using Newtonsoft.Json;

namespace Csp.Api.Model.Flex
{
    public class FwSerialAndSubscription
    {
        [JsonProperty("serialNumber")]
        public string SerialNumber { get; set; }

        [JsonProperty("subscriptions")]
        public string Subscriptions { get; set; }
    }
}
