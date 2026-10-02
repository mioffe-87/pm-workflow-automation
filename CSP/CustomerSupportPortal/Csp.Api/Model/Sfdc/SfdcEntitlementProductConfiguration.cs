using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Csp.Api.Model.Sfdc
{
    public class SfdcEntitlementProductConfiguration
    {
        [JsonProperty("region")]
        public string Region { get; set; }
    }
}
