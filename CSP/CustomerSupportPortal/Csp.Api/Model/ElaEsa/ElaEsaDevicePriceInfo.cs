using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.ElaEsa
{
    public class ElaEsaDevicePriceInfo
    {
        [JsonProperty("serialNumber")]
        public string SerialNumber { get; set; }

        [JsonProperty("authCode")]
        public string AuthCode { get; set; }

        [JsonProperty("agreementType")]
        public EElaEsaAgreementType AgreementType { get; set; }

        [JsonProperty("assetType")]
        public EElaEsaAssetType AssetType { get; set; }

        [JsonProperty("devicePrice")]
        public double? DevicePrice { get; set; }

        [JsonProperty("supportAccountId")]
        public string SupportAccountId { get; set; }

        [JsonProperty("devicePartNumber")]
        public string PartNumber { get; set; }

        [JsonProperty("deviceModel")]
        public string DeviceModel { get; set; }

        [JsonProperty("quantity")]
        public double? Quantity { get; set; }

        [JsonProperty("usageDate")]
        public DateTime? UsageDate { get; set; }

        [JsonProperty("isActiveInLast3Months")]
        public bool? IsActiveInLast3Months { get; set; }

        [JsonProperty("resellerName")]
        public string ResellerName { get; set; }

        [JsonProperty("swVersion")]
        public string SwVersion { get; set; }

        [JsonProperty("hwRegistrationDate")]
        public DateTime? RegistrationDate { get; set; }

        [JsonProperty("hwPurchaseDate")]
        public DateTime? PurchaseDate { get; set; }
    }
}
