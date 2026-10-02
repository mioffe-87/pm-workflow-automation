using Newtonsoft.Json;
using System.Collections.Generic;

namespace Csp.Api.Model
{
    public class OAuthApiRequest
    {
        [JsonProperty(PropertyName = "support_account_id")]
        public long SupportAccountId { get; set; }

        [JsonProperty(PropertyName = "user_account_id")]
        public long UserAccountId { get; set; }     

        [JsonProperty(PropertyName = "scopes")]
        public int[] Scopes { get; set; }      

        [JsonProperty(PropertyName = "client_id")]
        public string ClientId { get; set; }

        [JsonProperty(PropertyName = "action_type")]
        public string ActionType { get; set; }
       
    }
}
