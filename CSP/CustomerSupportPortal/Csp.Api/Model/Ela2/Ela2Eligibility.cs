using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.Ela2
{
    public class Ela2Eligibility
    {
        [JsonProperty("zingbox")]
        public EsaEla2EligibilityLink Ela2EligibilityIoTLink { get; set; }

        [JsonProperty("aperture")]
        public EsaEla2EligibilityLink Ela2EligibilitySaaSLink { get; set; }

        [JsonProperty("strata_insights")]
        public EsaEla2EligibilityLink Ela2EligibilityAiOpsLink { get; set; }

        [JsonProperty("strata_cloud_manager")]
        public EsaEla2EligibilityLink Ela2EligibilityScmLink { get; set; }

        [JsonProperty("dlp")]
        public EsaEla2EligibilityLink Ela2EligibilityDlpLink { get; set; }

        [JsonProperty("adnsr")]
        public EsaEla2EligibilityLink Ela2EligibilityAdnsrLink { get; set; }

        [JsonProperty("quantum")]
        public EsaEla2EligibilityLink Ela2EligibilityQuantumLink { get; set; }

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
