using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.Model.Ela2;

namespace Csp.Api.Model.Ela8
{
    public class Ela8Eligibility
    {
        [JsonProperty("adnsr")]
        public EsaEla2EligibilityLink Ela8EligibilityAdnsrLink { get; set; }
    }
}
