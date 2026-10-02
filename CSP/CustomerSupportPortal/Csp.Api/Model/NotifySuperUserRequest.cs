using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model
{
    public class NotifySuperUserRequest
    {
        [JsonProperty("requestedByUserAccountId")]
        public long UserAccountId { get; set; }

        [JsonProperty("supportAccountId")]
        public long SupportAccountId { get; set; }

        [JsonProperty("issues")]
        public List<long> Issues { get; set; }

        [JsonProperty("comment")]
        public string Comment { get; set; }

        [JsonProperty("superUsers")]
        public List<string> SuperUsers { get; set; }
    }
}
