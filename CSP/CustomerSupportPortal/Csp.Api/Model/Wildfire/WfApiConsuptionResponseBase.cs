using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.Wildfire
{

    public abstract class WfApiConsumptionResponseBase
    {
        [JsonProperty("supportAccountId")]
        public long SupportAccountId { get; set; }

        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("message", NullValueHandling = NullValueHandling.Ignore, Required = Required.AllowNull)]
        public string Message { get; set; }
    }
}
