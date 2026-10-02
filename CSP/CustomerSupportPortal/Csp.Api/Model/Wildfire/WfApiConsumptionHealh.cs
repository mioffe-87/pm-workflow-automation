using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.Wildfire
{
    /// <summary>
    /// <code>
    /// // GET ?supportAccountId=12345
    /// {
    ///     "supportAccountId": 12345,
    ///     "apiKeys": [ 
    ///         "9axxxxxxx59",
    ///         "9axxxxxxx57"
    ///     ]
    /// }
    /// </code>
    /// </summary>
    public class WfApiConsumptionHealh
    {
        [JsonProperty("operational")]
        public bool Operational { get; set; }

        [JsonProperty("message", NullValueHandling = NullValueHandling.Ignore, Required = Required.AllowNull)]
        public string Message { get; set; }

        [JsonProperty("details", NullValueHandling = NullValueHandling.Ignore, Required = Required.AllowNull)]
        public dynamic Details { get; set; }
    }
}
