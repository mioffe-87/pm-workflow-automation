using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.Flex
{
    public class CreditDetailFromEntitlementItemEntitlement
    {
        [JsonProperty(PropertyName = "entitlementId")]
        public string EntitlementId { get; set; }

        [JsonProperty(PropertyName = "productType")]
        public string ProductType { get; set; }

        [JsonProperty(PropertyName = "tokenId")]
        public long TokenId { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "productId")]
        public string ProductId { get; set; }

        [JsonProperty(PropertyName = "productName")]
        public string CreditName { get; set; }

        [JsonProperty(PropertyName = "sku")]
        public string Sku { get; set; }

        [JsonProperty(PropertyName = "genType")]
        public string GenType { get; set; }

        [JsonProperty(PropertyName = "orderDate")]
        public string OrderDate { get; set; }

        [JsonProperty(PropertyName = "startDate")]
        public string EffectiveDate { get; set; }

        [JsonProperty(PropertyName = "endDate")]
        public string ExpirationDate { get; set; }

        [JsonProperty(PropertyName = "instanceUrl")]
        public string InstanceUrl { get; set; }

        [JsonProperty(PropertyName = "licenseFeatures")]
        public IEnumerable<IDictionary<string, object>> LicenseFeatures { get; set; }

        [JsonProperty(PropertyName = "configuration")]
        public IEnumerable<IDictionary<string, object>> Configuration { get; set; }
    }
}
