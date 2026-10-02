using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.Ela2
{
    public class EsaEligibility
    {
        [JsonProperty("strata_cloud_manager")]
        public EsaEla2EligibilityLink EsaEligibilityScmLink { get; set; }

        [JsonProperty("sdwan_add_cgx")]
        public EsaEla2EligibilityLink EsaEligibilitySdWanAddCgxLink { get; set; }

        [JsonProperty("sdwan_add_coresec")]
        public EsaEla2EligibilityLink EsaEligibilitySdWanAddCoresecLink { get; set; }

        [JsonProperty("sdwan_zingbox")]
        public EsaEla2EligibilityLink EsaEligibilitySdWanZingboxLink { get; set; }

        [JsonProperty("sdwan_logging_service")]
        public EsaEla2EligibilityLink EsaEligibilitySdWanLoggingServiceLink { get; set; }
    }
}
