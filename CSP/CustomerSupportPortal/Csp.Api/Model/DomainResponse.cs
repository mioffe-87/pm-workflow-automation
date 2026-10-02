using Newtonsoft.Json;

namespace Csp.Api.Model
{
    public class DomainResponse
    {
        [JsonProperty("domain")]
        public string Domain { get; set; }

        [JsonProperty("isPublic")]
        public bool IsPublic { get; set; }

        [JsonProperty("isBlacklisted")]
        public bool IsBlacklisted { get; set; }
    }
}
