using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Csp.Api.Model.Sfdc
{
    public class SfdcEntitlementAccount
    {
        [JsonProperty("accountId")]
        public string AccountId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("culture")]
        public string Culture { get; set; }
    }
}
