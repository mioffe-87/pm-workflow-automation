using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.ExtendedExpertise
{
    public class ConsumerProductOkyoGardeParent
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("deviceId")]
        public long DeviceId { get; set; }

        [JsonProperty("tenantSerialNumber")]
        public string TenantSerialNumber { get; set; }

        [JsonProperty("partNumber")]
        public string PartNumber { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("totalQuantity")]
        public int? TotalQuantity { get; set; }

        [JsonProperty("totalActivatedQuantity")]
        public int? TotalActivatedQuantity { get; set; }

        [JsonProperty("totalAvailableQuantity")]
        public int? TotalAvailableQuantity { get; set; }

        [JsonProperty("serviceLevel")]
        public string ServiceLevel { get; set; }

        [JsonProperty("region")]
        public string Region { get; set; }

        [JsonProperty("expirationDate")]
        public DateTime? ExpirationDate { get; set; }

        [JsonProperty("licenses")]
        public IEnumerable<ConsumerProductOkyoGardeChild> Children { get; set; }

        public string Export()
        {
            return (!string.IsNullOrWhiteSpace(TenantSerialNumber) ? TenantSerialNumber : string.Empty) + ","
                + (!string.IsNullOrWhiteSpace(PartNumber) ? PartNumber : string.Empty) + ","
                + (!string.IsNullOrWhiteSpace(Description) ? Description : string.Empty) + ","
                + TotalQuantity + "," + TotalActivatedQuantity + "," + TotalAvailableQuantity + ","
                + (!string.IsNullOrWhiteSpace(Region) ? Region : string.Empty) + ","
                + (!string.IsNullOrWhiteSpace(ServiceLevel) ? ServiceLevel : string.Empty) + ","
                + (ExpirationDate.HasValue ? ExpirationDate.Value.ToString("d") : string.Empty) + ","
                + (Children != null && Children.Count() > 0 ? string.Join(",", Children.Select(c => c.Export()).ToArray()) : ConsumerProductOkyoGardeChild.ExportEmpty());
        }
    }
}
