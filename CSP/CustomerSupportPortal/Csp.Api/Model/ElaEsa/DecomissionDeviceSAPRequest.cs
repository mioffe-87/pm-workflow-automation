using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.ElaEsa
{
    public class DecomissionDeviceSAPRequest
    {
        [JsonProperty("Url")]
        public string Url { get; set; }
        [JsonProperty("OriginalPayload")]
        public ElaEsaDecomissionDevice[] Devices { get; set; }
    }
}
