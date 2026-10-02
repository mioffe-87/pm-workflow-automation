using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.Wildfire
{
    public class WfApiConsumptionResponseItem : WfApiConsumptionResponseItemBase
    {
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore, Required = Required.AllowNull)]
        public string Type { get; set; }

        [JsonProperty("apiKey", NullValueHandling = NullValueHandling.Ignore, Required = Required.AllowNull)]
        public string ApiKey { get; set; }
    }
}
