using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Csp.Api.Model.Sfdc
{
    public class SfdcEntitlementPartner
    {
        [JsonProperty("account")]
        public SfdcEntitlementAccount Account { get; set; }

        [JsonProperty("contact")]
        public SfdcEntitlementContact Contact { get; set; }
    }
}
