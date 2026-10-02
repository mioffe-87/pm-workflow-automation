using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.Wildfire
{
    public class WildfireApiNovaDataEntitlement
    {
        [JsonProperty("productId")]
        public string ProductId { get; set; }

        [JsonProperty("entitlementId")]
        public string EntitlementId { get; set; }

        [JsonProperty("entitlementGroupId")]
        public string EntitlementGroupId { get; set; }

        [JsonProperty("serialNumber")]
        public string SerialNumber { get; set; }

        [JsonProperty("isTrial")]
        public bool? IsTrial { get; set; }

        [JsonProperty("sku")]
        public string ModelName { get; set; }

        [JsonProperty("productType")]
        public string ProductType { get; set; }

        [JsonProperty("licenseData")]
        public WildfireApiNovaDataLicenseData LicenseData { get; set; }

        [JsonProperty("productName")]
        public string Description { get; set; }

        [JsonProperty("endDate")]
        public DateTime? ExpirationDate { get; set; }

        public WildfireAssetItem MapToAssets()
        {
            return new WildfireAssetItem
            {
                DeviceId = EntitlementId,
                SerialNumber = SerialNumber,
                IsTrial = IsTrial ?? false,
                ModelName = ModelName,
                Description = Description,
                Expiration = ExpirationDate.HasValue ? ExpirationDate.Value.ToString("d") : string.Empty,
                PurchaseQuantity = LicenseData == null ? null : LicenseData.PurchaseQuantity?.ToString("N0"),
                Quantity = LicenseData == null ? null : new WildfireAssetIteemQuantity
                {
                    Queries = LicenseData.Queries?.ToString("N0"),
                    Submissions = LicenseData?.Submissions?.ToString("N0")
                }
            };
        }

        public WildfireActivationLicense MapToActivationLicenses()
        {
            return new WildfireActivationLicense
            {
                ImageUrl = "image/PAN-WFAPI.png",
                Model = ModelName,
                Description = Description,
                PurchaseQuantity = LicenseData == null ? null : LicenseData.PurchaseQuantity?.ToString("N0"),
                Quantity = LicenseData == null ? null : new WildfireAssetIteemQuantity
                {
                    Queries = LicenseData.Queries?.ToString("N0"),
                    Submissions = LicenseData.Submissions?.ToString("N0")
                }
            };
        }
    }
}
