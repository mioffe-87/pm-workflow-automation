using Csp.Api.Model.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.Sfdc
{
    public class SfdcEntitlementSearchCriteria
    {
        [JsonIgnore]
        public SfdcEntitlementSearchType SearchType { get; set; }

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
        public string CspId { get; set; }

        [JsonProperty("serialNumber")]
        public string SerialNumber { get; set; }
    }
}