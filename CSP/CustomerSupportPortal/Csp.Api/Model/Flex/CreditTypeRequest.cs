using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.Flex
{
    public class CreditTypeRequest
    {
        [JsonProperty("sourceSupportAccountId")]
        public long SourceSupportAccountId { get; set; }

        [JsonProperty("targetSupportAccountId")]
        public long TargetSupportAccountId { get; set; }

        [JsonProperty("sourceTokenId")]
        public long SourceTokenId { get; set; }

        [JsonProperty("userAccountId")]
        public long UserAccountId { get; set; }
    }
}
