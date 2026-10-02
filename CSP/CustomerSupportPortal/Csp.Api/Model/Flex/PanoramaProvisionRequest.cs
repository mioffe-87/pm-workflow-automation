using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.Flex
{
    public class PanoramaProvisionRequest
    {
        [JsonProperty(PropertyName = "support_account_id")]
        public long SupportAccountId { get; set; }

        [JsonProperty(PropertyName = "user_account_id")]
        public long UserAccountId { get; set; }

        [JsonProperty(PropertyName = "auth_code")]
        public string AuthCode { get; set; }

        [JsonProperty(PropertyName = "quantity")]
        public int Quantity { get; set; }
    }
}
