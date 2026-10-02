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
    /// SUCCESS:
    /// {
    ///     "success": true,
    ///     "result": {
    ///         "api_keys": [
    ///             {
    ///                 "api_key": "wf_csp_api_usage_integration_test_key",
    ///                 "usage": [
    ///                     {
    ///                         "usage_by_time": [
    ///                             {
    ///                                 "query_limit": 100000,
    ///                                 "query_usage": 65218,
    ///                                 "upload_limit": 1000,
    ///                                 "upload_usage": 762,
    ///                                 "usage_time": "2022-06-01T00:00:00Z"
    ///                             },
    ///                        ],
    ///                         "usage_type": "proofpoint"
    ///                     }
    ///                 ]
    ///             }
    ///         ],
    ///         "csp_id": "249",
    ///         "status": "success"
    ///     }
    /// }
    /// ERROR:
    /// {
    ///     "success": true,
    ///     "result": {
    ///         "error": {
    ///             "error-message": "'Missing required field apikey'"
    ///         }
    ///     }
    /// }
    /// </code>
    /// </summary>
    public class WfApiExtConsumptionResponseResultItemUsage
    {
        [JsonProperty("usage_by_time")]
        public IEnumerable<WfApiExtConsumptionResponseResultItemUsageData> Data { get; set; }

        [JsonProperty("usage_type")]
        public string Type { get; set; }
    }
}
