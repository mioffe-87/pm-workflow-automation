using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.Ion
{
    public class IonItem
    {
        private const string COMMA_SEPARATOR = ",";

        [JsonProperty("deviceId")]
        public string DeviceId { get; set; }

        [JsonProperty("serialNumber")]
        public string SerialNumber { get; set; }

        [JsonProperty("licenseId")]
        public string LicenseId { get; set; }

        [JsonProperty("authCode")]
        public string AuthCode { get; set; }

        [JsonProperty("modelName")]
        public string ModelName { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("expiration")]
        public DateTime? Expiration { get; set; }

        [JsonProperty("quantity")]
        public int Quantity { get; set; }

        [JsonIgnore]
        public bool IsParent { get; set; }

        [JsonProperty("licenses", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<IonItem> Licenses { get; set; }

        [JsonProperty("tenantId")]
        public string TenantId { get; set; }

        public string Export(EDeviceType deviceType)
        {
            return (!string.IsNullOrWhiteSpace(TenantId) ? TenantId : string.Empty) + COMMA_SEPARATOR
                + (deviceType != EDeviceType.AddOns
                    ? (!string.IsNullOrWhiteSpace(SerialNumber) ? SerialNumber : string.Empty) + COMMA_SEPARATOR
                    : string.Empty)
                + (!string.IsNullOrWhiteSpace(AuthCode) ? AuthCode : string.Empty) 
                + COMMA_SEPARATOR + (!string.IsNullOrWhiteSpace(ModelName) ? ModelName : string.Empty) 
                + COMMA_SEPARATOR + (!string.IsNullOrWhiteSpace(Description) ? "\"" + Description + "\"": string.Empty) 
                + COMMA_SEPARATOR + (Expiration.HasValue ? "\"" + Expiration.Value + "\"" : string.Empty) 
                + (deviceType != EDeviceType.Physical ? COMMA_SEPARATOR + (Quantity > 0 ? Quantity.ToString() : string.Empty) : string.Empty);
        }
    }
}
