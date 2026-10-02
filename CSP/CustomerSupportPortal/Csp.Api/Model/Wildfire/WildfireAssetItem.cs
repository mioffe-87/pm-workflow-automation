using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.Wildfire
{
    public class WildfireAssetItem
    {
        private const string COMMA_SEPARATOR = ",";

        [JsonProperty("deviceId")]
        public string DeviceId { get; set; }

        [JsonProperty("serialNumber")]
        public string SerialNumber { get; set; }

        [JsonProperty("isTrial")]
        public bool IsTrial { get; set; }

        [JsonProperty("modelName")]
        public string ModelName { get; set; }

        [JsonProperty("puchaseQuantity")]
        public string PurchaseQuantity { get; set; }

        [JsonProperty("quantity")]
        public WildfireAssetIteemQuantity Quantity { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("expiration")]
        public string Expiration { get; set; }

        [JsonProperty("supportLicenseType")]
        public string SupportLicenseType { get; set; }

        public string Export()
        {
            return SerialNumber
                + COMMA_SEPARATOR + IsTrial
                + COMMA_SEPARATOR + (!string.IsNullOrWhiteSpace(ModelName) ? ModelName : string.Empty)
                + COMMA_SEPARATOR + (Quantity != null ? "\"" + Quantity + "\"" : string.Empty)
                + COMMA_SEPARATOR + (!string.IsNullOrWhiteSpace(Description) ? "\"" + Description + "\"" : string.Empty)
                + COMMA_SEPARATOR + (!string.IsNullOrWhiteSpace(Expiration) ? "\"" + Expiration + "\"" : string.Empty)
                + COMMA_SEPARATOR + (!string.IsNullOrWhiteSpace(SupportLicenseType) ? SupportLicenseType : string.Empty);
        }
    }
}
