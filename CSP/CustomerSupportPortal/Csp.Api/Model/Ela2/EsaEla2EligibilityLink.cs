using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.Ela2
{
    public class EsaEla2EligibilityLink
    {
        [JsonProperty("url")]
        public string EsaEla2EligibilityUrl { get; set; }
    }
}
