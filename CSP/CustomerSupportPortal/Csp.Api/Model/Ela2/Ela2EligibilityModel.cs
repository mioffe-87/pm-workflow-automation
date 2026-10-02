using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.Model.Ela8;

namespace Csp.Api.Model.Ela2
{
    public class Ela2EligibilityModel
    {
        [JsonProperty("support_account_id")]
        public int SupportAccountId { get; set; }

        [JsonProperty("ela")]
        public Ela2Eligibility Ela2Links { get; set; }

        [JsonProperty("esa")]
        public EsaEligibility EsaLinks { get; set; }

        [JsonProperty("ela8")]
        public Ela8Eligibility Ela8Links { get; set; }
    }
}
