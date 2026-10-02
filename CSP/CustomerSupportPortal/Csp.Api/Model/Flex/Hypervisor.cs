using Newtonsoft.Json;

namespace Csp.Api.Model.Flex
{
    public class Hypervisor
    {
        [JsonProperty(PropertyName = "value")]
        public string HypervisorVersion { get; set; }

        [JsonProperty(PropertyName = "label")]
        public string DisplayValue { get; set; }
    }
}
