using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.ExtendedExpertise
{
    public class ExtendedExpertiseCoreData
    {
        [JsonProperty("isSuperUser")]
        public bool IsSuperUser { get; set; }

        [JsonProperty("availableCreditBalance")]
        public string AvailableCreditBalance { get; set; }

        [JsonProperty("googleFormUrl")]
        public string GoogleFormUrl { get; set; }

        [JsonProperty("creditMenuUrl")]
        public string CreditMenuUrl { get; set; }
    }
}
