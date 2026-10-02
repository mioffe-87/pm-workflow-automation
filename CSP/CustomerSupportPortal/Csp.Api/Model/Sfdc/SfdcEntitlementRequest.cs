using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.Sfdc
{
    public class SfdcEntitlementRequest
    {
        [JsonProperty("entitlementId")]
        public string EntitlementId { get; set; }

        [JsonProperty("entitlementGroupId")]
        public string EntitlementGroupId { get; set; }

        [JsonProperty("accountId")]
        public string AccountId { get; set; }

        [JsonProperty("accountName")]
        public string AccountName { get; set; }

        [JsonProperty("contactEmail")]
        public string ContactEmail { get; set; }

        [JsonProperty("contactId")]
        public string ContactId { get; set; }

        [JsonProperty("cspId")]
        public long? CspId { get; set; }

        [JsonProperty("cspIds")]
        public string[] CspIds { get; set; }

        [JsonProperty("serialNumber")]
        public string SerialNumber { get; set; }

        [JsonProperty("tenantId")]
        public string TenantId { get; set; }

        [JsonProperty("tenantIds")]
        public string TenantIds { get; set; }

        [JsonProperty("ascAccountId")]
        public string AscAccountId { get; set; }
    }
}
