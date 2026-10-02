using Newtonsoft.Json;

namespace Csp.Api.Model
{
    public class CdlNotificationRequest
    {
        [JsonProperty("serialNumber")]
        public string SerialNumber { set; get; }        

        [JsonProperty("deviceId")]
        public long DeviceId { set; get; } 

        [JsonProperty("eventType")]
        public string EventType { set; get; }

        [JsonProperty("oldSupportAccountId")]
        public long OldSupportAccountId { set; get; }        

        [JsonProperty("newSupportAccountId")]
        public long NewSupportAccountId { set; get; }

        [JsonProperty("productType")]
        public string ProductType { set; get; }

    }
}
