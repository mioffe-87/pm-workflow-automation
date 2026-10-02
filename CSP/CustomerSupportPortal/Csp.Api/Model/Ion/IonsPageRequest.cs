using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.Ion
{
    public class IonsPageRequest
    {
        //[JsonProperty("deviceType")]
        //public EDeviceType DeviceType { get; set; }

        [JsonProperty("supportAccountId")]
        public long SupportAccountId { get; set; }

        [JsonProperty("userAccountId")]
        public long UserAccountId { get; set; }
    }
}
