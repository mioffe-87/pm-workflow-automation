using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model
{
    public class SupportCaseNavigationRequest
    {
        [JsonProperty(PropertyName = "userAccountId")]
        public long UserAccountId { get; set; }

        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }

        [JsonProperty(PropertyName = "supportAccountId")]
        public long SupportAccountId { get; set; }

        [JsonProperty(PropertyName = "companyName")]
        public string CompanyName { get; set; }

        [JsonProperty(PropertyName = "isSidenav")]
        public bool IsSidenav { get; set; }

        [JsonProperty(PropertyName = "path")]
        public string path { get; set; }
    }
}
