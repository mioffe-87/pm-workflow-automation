using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.Wildfire
{
    /// <summary>
    /// 
    /// <code>
    /// {
    ///     "supportAccountId": 12345,
    ///     "type": "public", // proofpoint, public or NULL
    ///     "apiKeys": [ // NULL, empty, collection of one, or collection of many
    ///         "9axxxxxxx59",
    ///         "9axxxxxxx57"
    ///     ], 
    ///     "startDate": "2021-12-16T00:00:00Z",
    ///     "endDate": "2022-03-15T00:00:00Z"
    /// }
    /// </code>
    /// </summary>
    public class WfApiConsumptionRequest
    {
        [JsonProperty("supportAccountId")]
        public long SupportAccountId { get; set; }

        [JsonProperty("userAccountId")]
        public long UserAccountId { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("apiKeys")]
        public IEnumerable<string> ApiKeys { get; set; }

        [JsonProperty("startDate")]
        public DateTime? StartDate { get; set; }

        [JsonProperty("EndDate")]
        public DateTime? EndDate { get; set; }
    }
}
