using Newtonsoft.Json;

namespace Csp.Api.Model.Flex
{
    public class PanOsVersion
    {
        [JsonProperty(PropertyName = "value")]
        public string PanOsVersionValue { get; set; }

        [JsonProperty(PropertyName = "label")]
        public string PanOsVersionLabel { get; set; }
    }
}

