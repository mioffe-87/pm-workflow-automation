using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.Wildfire
{
    public class WfApiConsumptionResponseItemBase
    {
        [JsonProperty("time", Required = Required.DisallowNull)]
        public DateTime? Date { get; set; }

        [JsonProperty("queryUsage", NullValueHandling = NullValueHandling.Ignore, Required = Required.AllowNull)]
        public int? QueryUsage { get; set; }

        [JsonProperty("uploadUsage", NullValueHandling = NullValueHandling.Ignore, Required = Required.AllowNull)]
        public int? UploadUsage { get; set; }

        [JsonProperty("queryLimit", NullValueHandling = NullValueHandling.Ignore, Required = Required.AllowNull)]
        public int? QueryLimit { get; set; }

        [JsonProperty("uploadLimit", NullValueHandling = NullValueHandling.Ignore, Required = Required.AllowNull)]
        public int? UploadLimit { get; set; }
    }
}
