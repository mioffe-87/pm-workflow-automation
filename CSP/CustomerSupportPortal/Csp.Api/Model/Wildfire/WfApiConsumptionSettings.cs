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
    public class WfApiConsumptionSettings : WfApiConsumptionResponseBase
    {
        [JsonProperty("apiKeys", NullValueHandling = NullValueHandling.Ignore, Required = Required.AllowNull)]
        public IEnumerable<string> ApiKeys { get; set; }
    }
}
