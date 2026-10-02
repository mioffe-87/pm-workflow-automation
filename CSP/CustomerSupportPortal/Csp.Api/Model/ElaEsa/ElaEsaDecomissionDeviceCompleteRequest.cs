using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.ElaEsa
{
    public class ElaEsaDecomissionDeviceCompleteRequest
    {
        [JsonProperty("sessionId")]
        public string SessionId { get; set; }

        [JsonProperty("devices")]
        public ElaEsaDecomissionDeviceCompleteDeviceRequest[] Devices { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }
}
