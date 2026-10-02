using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Csp.Api.Model.Ela2
{
    public class ElaConversionStatusBase
    {
        public const string URL4TOADVURL = "URL4TOADVURL";

        [JsonProperty("supportAccountId")]
        public long SupportAccountId { get; set; }

        [JsonProperty("userAccountId")]
        public long UserAccountId { get; set; }

        [JsonProperty("conversionType")]
        public string ConversionType { get; set; }
    }
}
