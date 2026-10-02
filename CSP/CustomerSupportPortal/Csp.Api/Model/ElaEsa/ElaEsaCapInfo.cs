using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.ElaEsa
{
    public class ElaEsaCapInfo
    {
        [JsonProperty("usageType")]
        public string UsageType { get; set; }

        /// <summary>
        /// Type of the contract: ELA or ESA. 
        /// 
        /// </summary>
        [JsonProperty("type")]
        public EElaEsaAgreementType Type { get; set; }

        /// <summary>
        /// ELA or ESA Master authcode.
        /// </summary>
        [JsonProperty("authCode")]
        public string MasterAuthCode { get; set; }

        /// <summary>
        /// Money paid by client for the contract.
        /// </summary>
        [JsonProperty("contractEstate")]
        public double? ContractEstate { get; set; }

        /// <summary>
        /// ?
        /// </summary>
        [JsonProperty("projectedEstate")]
        public double? ProjectedEstate { get; set; }

        /// <summary>
        /// Flag indicates if Hardcap is applied to the contract.
        /// If not, the Hardcap calculations are ignored. 
        /// </summary>
        [JsonProperty("applyHardcap")]
        public bool ApplyHardcap { get; set; }

        /// <summary>
        /// Flag indicates if Hard Cap is reached.
        /// </summary>
        [JsonProperty("isHardcapReached")]
        public bool IsHardcapReached { get; set; }

        /// <summary>
        /// Hardware cap to use in calculations whether to allow a new device registration or not.
        /// Applied only if ApplyHardcap is TRUE.
        /// </summary>
        [JsonProperty("hardwareCap", NullValueHandling = NullValueHandling.Ignore)]
        public double? HardwareCap { get; set; }

        /// <summary>
        /// Exception prop indicated if HardwareCap is in action.
        /// If TRUE, there is HardwareCap is NULL; otherwise - HardwareCap has a value.
        /// </summary>
        [JsonProperty("exception", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Exception { get; set; }

        /// <summary>
        /// Installed base before ELA/EAS contract. 
        /// </summary>
        //[JsonProperty("hardwareInstalledBase")]
        //public double? HardwareInstalledBase { get; set; }

        /// <summary>
        /// Each installed device costs money. This prop aggregates the money already spent for 
        /// the all installed devices from the ContractEstate/HardwareCap allowance, excluding the cost of the device 
        /// (if any) under review for the registration.
        /// </summary>
        [JsonProperty("hardwareInstalledPrice")]
        public double? HardwareInstalledPrice { get; set; }

        /// <summary>
        /// Number of the installed devices linked to this contract, excluding the cost of the device 
        /// (if any) under review for the registration.
        /// </summary>
        [JsonProperty("hardwareInstalledNumber")]
        public double? HardwareInstalledNumber { get; set; }

        /// <summary>
        /// Consumption percentage.
        /// </summary>
        [JsonProperty("consumptionPercentage")]
        public double? ConsumptionPercentage { get; set; }

        /// <summary>
        /// Date of report data.
        /// </summary>
        [JsonProperty("date")]
        public DateTime? Date { get; set; }

        /// <summary>
        /// ELA/ESA contract start date.
        /// </summary>
        [JsonProperty("contractStartDate")]
        public DateTime? ContractStartDate { get; set; }

        /// <summary>
        /// ELA/ESA contract end date.
        /// </summary>
        [JsonProperty("contractEndDate")]
        public DateTime? ContractEndDate { get; set; }

        public ElaEsaCapInfoWithNewDevice ToElaEsaCapInfoWithNewDevice(ElaEsaDevicePriceInfo device = null)
        {
            ElaEsaCapInfoWithNewDevice theDevice = new ElaEsaCapInfoWithNewDevice
            {
                Type = Type,
                MasterAuthCode = MasterAuthCode,
                ContractStartDate = ContractStartDate,
                ContractEndDate = ContractEndDate,
                ContractEstate = ContractEstate,
                ProjectedEstate = ProjectedEstate,
                ApplyHardcap = ApplyHardcap,
                HardwareCap = HardwareCap,
                Exception = Exception, 
                HardwareInstalledPrice = HardwareInstalledPrice,
                HardwareInstalledNumber = HardwareInstalledNumber,
                HardwareInstalledBaseAfterInstallation = HardwareInstalledPrice,
                HardwareInstalledNumberAfterInstallation = device != null
                    && (device.AgreementType == Type
                    || device.AgreementType == EElaEsaAgreementType.All)
                    && HardwareInstalledNumber.HasValue
                    ? HardwareInstalledNumber.Value + 1
                    : HardwareInstalledNumber
            };
            if (device != null
                && (device.AgreementType == Type 
                || device.AgreementType == EElaEsaAgreementType.All)
                && device.DevicePrice.HasValue
                && device.DevicePrice.Value > 0
                && theDevice.HardwareInstalledBaseAfterInstallation.HasValue)
            {
                theDevice.HardwareInstalledBaseAfterInstallation = theDevice.HardwareInstalledBaseAfterInstallation.Value + device.DevicePrice.Value;
                if (theDevice.HardwareCap.HasValue
                    && theDevice.HardwareInstalledBaseAfterInstallation.Value > theDevice.HardwareCap.Value)
                {
                    theDevice.Reason = "Registering the new device exceeds Hardcap by $" + (theDevice.HardwareInstalledBaseAfterInstallation.Value - theDevice.HardwareCap.Value) + ".";
                    theDevice.HardCapExcess = true;
                }
                else if (theDevice.ContractEstate.HasValue
                    && theDevice.HardwareInstalledBaseAfterInstallation.Value > theDevice.ContractEstate.Value)
                {
                    theDevice.Reason = "Registering the new device exceeds Cap by $" + (theDevice.HardwareInstalledBaseAfterInstallation.Value - theDevice.ContractEstate.Value) + ", but does not exceed Hardcap.";
                    theDevice.HardCapExcess = false;
                }
            }
            return theDevice;
        }
    }
}
