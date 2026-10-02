using Newtonsoft.Json;

namespace Csp.Api.Model
{
    public class LoggingRegions
    {
        [JsonIgnore]
        public string Region;

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("display_name")] 
        public string DisplayName { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }
    }

    public enum LoggingRegionStatus
    {
        ACTIVE,
        INACTIVE
    }
}
