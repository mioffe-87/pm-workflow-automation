using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model
{
    public class MembershipExtensionRequest
    {
        [JsonProperty("membershipId")]
        public long membershipId { get; set; }

        [JsonProperty("userAccountId")]
        public long UserAccountId { get; set; }

        [JsonProperty("supportAccountId")]
        public long SupportAccountId { get; set; }

        [JsonProperty("months")]
        public int Months { get; set; }

        [JsonProperty("extendAll")]
        public bool ExtendAll { get; set; }
    }
}
