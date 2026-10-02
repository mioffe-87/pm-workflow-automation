using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.ExtendedExpertise
{
    public class ExtendedExpertiseResponseData
    {
        [JsonProperty("assets")]
        public IEnumerable<ExtendedExpertiseAsset> Assets { get; set; }

        [JsonProperty("usageTransactions")]
        public IEnumerable<ExtendedExpertiseUsageTransaction> UsageTransactions { get; set; }
    }
}
