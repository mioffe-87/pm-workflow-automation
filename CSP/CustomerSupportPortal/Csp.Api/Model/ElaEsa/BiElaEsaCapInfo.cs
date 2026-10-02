using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.ElaEsa
{
    public class BiElaEsaCapInfo
    {
        [JsonProperty("usageType")]
        public string UsageType { get; set; }

        [JsonProperty("eaType")]
        public string EaType { get; set; }

        [JsonProperty("monthEndDate")]
        public DateTime? MonthEndDate { get; set; }

        [JsonProperty("authCode")]
        public string MasterAuthCode { get; set; }

        [JsonProperty("usageDate")]
        public DateTime? UsageDate { get; set; }

        [JsonProperty("hwEstateCapLevel")]
        public double? HwEstateCapLevel { get; set; }

        private bool exception = false;
        [JsonProperty("exception")]
        public bool? Exception { get { return exception; } set { exception = value.HasValue && !value.Value; } } // Reverse logic in the setter to make the definition of Exception (do not show HardCap) make more sense.

        [JsonProperty("totalRegistered")]
        public double? TotalRegistered { get; set; }

        [JsonProperty("hwEstateProjected")]
        public double? HwEstateProjected { get; set; }

        [JsonProperty("hwEstateCurrentConsumingELAUnits")]
        public double? HwEstateCurrentConsumingElaUnits { get; set; }

        [JsonProperty("isBounded")]
        public bool IsBound { get; set; }

        [JsonProperty("isELAOverconsumed")]
        public bool IsElaOverconsumed { get; set; }

        [JsonProperty("elaConsumptionPercentage")]
        public double? ElaConsumptionPercentage { get; set; }

        [JsonProperty("inactiveDevices")]
        public double? InactiveDevices { get; set; }

        [JsonProperty("contractStartDate")]
        public DateTime? ContractStartDate { get; set; }

        [JsonProperty("contractEndDate")]
        public DateTime? ContractEndDate { get; set; }

        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(UsageType)
                && !string.IsNullOrWhiteSpace(EaType)
                && !string.IsNullOrWhiteSpace(MasterAuthCode)
                // && !string.IsNullOrWhiteSpace(UsageDate)
                // && HwEstateCapLevel.HasValue
                // && TotalRegistered.HasValue
                // && HwEstateProjected.HasValue
                // && HwEstateCurrentConsumingElaUnits.HasValue
                // && ElaConsumptionPercentage.HasValue
                // && InactiveDevices.HasValue
                && MonthEndDate.HasValue;
        }

        public ElaEsaCapInfo ToElaEsaCapInfo()
        {
            return new ElaEsaCapInfo
            {
                UsageType = UsageType, 
                ApplyHardcap = IsBound,
                ContractEstate = HwEstateCapLevel * ElaConsumptionPercentage,
                HardwareCap = exception ? null : HwEstateCapLevel,
                Exception = exception ? (bool?)true : null,
                HardwareInstalledPrice = TotalRegistered,
                HardwareInstalledNumber = HwEstateCurrentConsumingElaUnits,
                MasterAuthCode = MasterAuthCode,
                ProjectedEstate = HwEstateProjected,
                IsHardcapReached = IsElaOverconsumed,
                ConsumptionPercentage = ElaConsumptionPercentage,
                Date = MonthEndDate?.Date,
                ContractStartDate = ContractStartDate,
                ContractEndDate = ContractEndDate,
                Type = EaType == "ELA" ? EElaEsaAgreementType.Ela : EaType == "ESA" ? EElaEsaAgreementType.Esa : EElaEsaAgreementType.All,
            };
        }
    }
}
