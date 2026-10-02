using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum SupportEventType
    {
        dayOneConfig,
    }

    public class SupportEventData
    {
        [JsonProperty(PropertyName="email")]
        public string Email;
        [JsonProperty(PropertyName = "serialNumber")]
        public string SerialNumber;
        [JsonProperty(PropertyName = "supportAccountId")]
        public long SupportAccountId;
        [JsonProperty(PropertyName = "templateVersion")]
        public string TemplateVersion;
        [JsonProperty(PropertyName = "osVersion")]
        public string OSVersion;
        [JsonProperty(PropertyName = "timestamp")]
        public long Timestamp;
        [JsonProperty(PropertyName = "status")]
        public string Status;
        [JsonProperty(PropertyName = "userAccountId")]
        public long UserAccountId;
    }

    public class SupportEvent
    {
        public Guid GUID;
        [JsonProperty(PropertyName = "type")]
        public SupportEventType Type;
        [JsonProperty(PropertyName = "data")]
        public SupportEventData Data;

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public StringContent ToStringContent()
        {
            return new System.Net.Http.StringContent(this.ToJson(), Encoding.UTF8, "application/json");
        }
    }
}
