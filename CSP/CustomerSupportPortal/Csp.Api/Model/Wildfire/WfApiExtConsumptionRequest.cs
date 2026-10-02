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
    ///   "csp_id": "249",
    ///   "apikey": "wf_csp_integration_test"
    /// }
    /// </code>
    /// </summary>
    public class WfApiExtConsumptionRequest
    {
        [JsonProperty("csp_id")]
        public string CspId { get; set; }

        [JsonProperty("apikey")]
        public string ApiKey { get; set; }

        [JsonProperty("start_date", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? StartDate { get; set; }

        [JsonProperty("end_date", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? EndDate { get; set; }
    }
}
