using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Csp.Api.Model.Ela2
{
    public class ElaConversionStatusData
    {
        [JsonProperty("requestId")]
        public int RequestId { get; set; }

        [JsonProperty("supportAccountId")]
        public int SupportAccountId { get; set; }

        [JsonProperty("userAccountId")]
        public int UserAccountId { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("conversionType")]
        public string ConversionType { get; set; }
    }
}
