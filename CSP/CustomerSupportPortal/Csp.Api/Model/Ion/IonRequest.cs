using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.Ion
{
    public class IonRequest
    {
        /// <summary>
        /// Flag indicates Virtual (TRUE), Phisical (FALSE) or Add-Ons (NULL) IONs request.
        /// </summary>
        [JsonProperty("deviceType")]
        public EDeviceType DeviceType { get; set; }

        [JsonProperty("supportAccountId")]
        public long SupportAccountId { get; set; }

        [JsonProperty("userAccountId")]
        public long UserAccountId { get; set; }

        [JsonProperty("tenantId")]
        public string TenantId { get; set; }

        [JsonProperty("search_field")]
        public string SearchField { get; set; }

        [JsonProperty("search_value")]
        public string SearchValue { get; set; }

        [JsonProperty("sort_field")]
        public string SortField { get; set; }

        [JsonProperty("sort_direction")]
        public string SortOrder { get; set; }

        [JsonProperty("page")]
        public int Page { get; set; }

        [JsonProperty("size")]
        public int PerPage { get; set; }
    }
}
