using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model
{
    
    public class BetaAccountsResponse
    {
        [JsonProperty("betaAccounts")]
        public List<BetaAccount> BetaAccounts { get; set; }
    }

    public class BetaAccount
    {
        [JsonProperty(PropertyName = "betaFeatureId")]
        public int BetaFeatureId { get; set; }

        [JsonProperty(PropertyName = "supportAccountId")]
        public long SupportAccountId { get; set; }
    }
}
