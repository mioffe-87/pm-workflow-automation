using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Csp.Api.Model.Sfdc
{
    public class SfdcEntitlementLink
    {
        [JsonProperty("serviceType")]
        public string ServiceType { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("multiTenantUrls")]
        public List<string> MultiTenantUrls { get; set; }
    }
}
