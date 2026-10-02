using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.ElaEsa
{
    public class ElaEsaCapInfoWithNewDevice : ElaEsaCapInfo
    {
        /// <summary>
        /// Each installed device costs money. This prop aggregates the money already spent for the all 
        /// installed devices from the ContractEstate/HardwareCap allowance, including the cost of the 
        /// device (if any) under review for the registration.
        /// </summary>
        [JsonProperty("hardwareInstalledBaseAfterInstallation")]
        public double? HardwareInstalledBaseAfterInstallation { get; set; }

        /// <summary>
        /// Number of the installed devices linked to this contract, including the cost of the device 
        /// (if any) under review for the registration.
        /// </summary>
        [JsonProperty("hardwareInstalledNumberAfterInstallation")]
        public double? HardwareInstalledNumberAfterInstallation { get; set; }

        [JsonProperty("reason")]
        public string Reason { get; set; }

        /// <summary>
        /// Flag indicates if a device for Hardcap excess.
        /// </summary>
        /// <returns>
        /// NULL - Hardcap is N/A (ApplyHardcap is FALSE) or not exceeded.
        /// FALSE - Cap is exceeded.
        /// TRUE - Hardcap is exceeded.
        /// </returns>
        [JsonProperty("hardCapExcess")]
        public bool? HardCapExcess { get; set; }
    }
}
