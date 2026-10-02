using Newtonsoft.Json;

namespace Csp.Api.Model.Sfdc
{
    public class SfdcAscAccount
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("accountId")]
        public string AccountId { get; set; }
    }
}
