using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.ElaEsa
{
    public class BiElaEsaDevicePriceInfo
    {
        [JsonProperty("serialNumber")]
        public string SerialNumber { get; set; }

        [JsonProperty("supportAccountId")]
        public string SupportAccountId { get; set; }

        [JsonProperty("devicePartNumber")]
        public string PartNumber { get; set; }

        [JsonProperty("deviceModel")]
        public string DeviceModel { get; set; }

        [JsonProperty("quantity")]
        public double? Quantity { get; set; }

        [JsonProperty("authcode")]
        public string AuthCode { get; set; }

        [JsonProperty("usageDate")]
        public DateTime? UsageDate { get; set; }

        [JsonProperty("hwEstateActual")]
        public double? DevicePrice { get; set; }

        [JsonProperty("isActiveInLast3Months")]
        public bool? IsActiveInLast3Months { get; set; }

        [JsonProperty("resellerName")]
        public string ResellerName { get; set; }

        [JsonProperty("eaType")]
        public string EaType { get; set; }

        [JsonProperty("swVersion")]
        public string SwVersion { get; set; }

        [JsonProperty("hwRegistrationDate")]
        public DateTime? RegistrationDate { get; set; }

        [JsonProperty("hwPurchaseDate")]
        public DateTime? PurchaseDate { get; set; }

        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(SupportAccountId)
                && Quantity.HasValue
                && !string.IsNullOrWhiteSpace(AuthCode)
                // && DevicePrice.HasValue
                // && UsageDate.HasValue
                // && RegistrationDate.HasValue
                && (EaType == "ELA" 
                || EaType == "ESA")
                && !string.IsNullOrWhiteSpace(SerialNumber);
        }

        public ElaEsaDevicePriceInfo ToElaEsaDevicePriceInfo()
        {
            return new ElaEsaDevicePriceInfo
            {
                AgreementType = EaType == "ESA" ? EElaEsaAgreementType.Esa : EElaEsaAgreementType.Ela,
                AssetType = EElaEsaAssetType.Hw,
                AuthCode = AuthCode,
                DevicePrice = DevicePrice,
                SerialNumber = SerialNumber,
                DeviceModel = DeviceModel,
                IsActiveInLast3Months = IsActiveInLast3Months,
                PartNumber = PartNumber,
                PurchaseDate = PurchaseDate,
                Quantity = Quantity,
                RegistrationDate = RegistrationDate,
                ResellerName = ResellerName,
                SupportAccountId = SupportAccountId,
                SwVersion = SwVersion,
                UsageDate = UsageDate
            };
        }
    }
}
