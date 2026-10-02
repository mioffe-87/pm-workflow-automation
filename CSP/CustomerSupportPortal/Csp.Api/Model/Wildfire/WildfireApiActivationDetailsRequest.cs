using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.Wildfire
{
    public class WildfireApiActivationDetailsRequest
    {
        [JsonProperty("userAccountId")]
        public long UserAccountId { get; set; }


        [JsonProperty("hash")]
        public string Hash { get; set; }
    }
}
