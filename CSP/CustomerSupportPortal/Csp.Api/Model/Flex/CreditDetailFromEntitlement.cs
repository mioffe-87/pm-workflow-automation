using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.Flex
{
    public class CreditDetailFromEntitlement
    {
        [JsonProperty(PropertyName = "entitlementGroupId")]
        public string EntitlementGroupId { get; set; }

        [JsonProperty(PropertyName = "salesAccountId")]
        public string SalesAccountId { get; set; }

        [JsonProperty(PropertyName = "salesAccountName")]
        public string SalesAccountName { get; set; }

        [JsonProperty(PropertyName = "supportAccountId")]
        public long? SupportAccountId { get; set; }

        [JsonProperty(PropertyName = "ascAccountId")]
        public string AscAccountId { get; set; }

        [JsonProperty(PropertyName = "supportType")]
        public string SupportType { get; set; }

        [JsonProperty(PropertyName = "entitlements")]
        public IEnumerable<CreditDetailFromEntitlementItemEntitlement> Entitlements { get; set; }
    }
}
