using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model
{
    public class SupportMember
    {
        [JsonProperty("supportAccountId")]
        public long SupportAccountId { get; set; }
        [JsonProperty("hasMembers")]
        public bool MemberExists { get; set; }
    }
}
