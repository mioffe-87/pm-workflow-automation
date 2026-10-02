using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Csp.Api.Model
{
    public class PortalDeviceListResponse:BaseJobResponse
    {
        [JsonProperty("eligiblity_info")]
        public Dictionary<string, EligibilityStatus> EligiblityInfo { get; set; }
    }

    public enum EligibilityStatus
    {
        No,
        Yes
    }
}
