using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Csp.Api.Model.Sfdc
{
    public class SfdcEntitlementResponseError
    {
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("data")]
        public string Data { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
    }
}
