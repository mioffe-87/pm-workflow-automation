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
    /// {
    ///     "success": true,
    ///     "items": [
    ///         {
    ///             "time": "2022-03-16T00:00:00Z",
    ///             "queryUsage": 5890,
    ///             "uploadUsage": 450,
    ///             "queryLimit": 10000,
    ///             "uploadLimit": 1000
    ///         },
    ///     ]
    /// }
    /// </code>
    /// </summary>
    public class WfApiConsumptionResponse : WfApiConsumptionResponseBase
    {
        [JsonProperty("items", NullValueHandling = NullValueHandling.Ignore, Required = Required.AllowNull)]
        public IEnumerable<WfApiConsumptionResponseItemBase> Items { get; set; }
    }
}
