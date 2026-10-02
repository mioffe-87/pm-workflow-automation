using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Csp.Api.Model.Sfdc
{
    public class SfdcEntitlementSupportAccount
    {
        [JsonProperty("cspId")]
        public string SupportAccountId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
