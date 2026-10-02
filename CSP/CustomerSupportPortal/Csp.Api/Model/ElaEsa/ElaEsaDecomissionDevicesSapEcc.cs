using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.ElaEsa
{
    public class ElaEsaDecomissionDevicesSapEcc
    {
        [JsonProperty("SessionID")]
        public string SessionId { get; set; }

        [JsonProperty("CSPData")]
        public IEnumerable<ElaEsaDecomissionDevicesSapDevice> Devices { get; set; }

        public ElaEsaDecomissionDevicesSapEcc()
        {
        }

        public ElaEsaDecomissionDevicesSapEcc(long sessionId, IEnumerable<ElaEsaDecomissionDevice> device)
        {
            SessionId = sessionId.ToString();
            Devices = device?.Select(d => new ElaEsaDecomissionDevicesSapDevice(d)).ToArray();
        }
    }

    public class ElaEsaRecycleDevicesSapEcc
    {
        [JsonProperty("SessionID")]
        public string SessionId { get; set; }

        [JsonProperty("CSPRMAData")]
        public IEnumerable<ElaEsaDecomissionDevicesSapDevice> Devices { get; set; }

        public ElaEsaRecycleDevicesSapEcc()
        {
        }

        public ElaEsaRecycleDevicesSapEcc(long sessionId, IEnumerable<ElaEsaDecomissionDevice> device)
        {
            SessionId = sessionId.ToString();
            Devices = device?.Select(d => new ElaEsaDecomissionDevicesSapDevice(d)).ToArray();
        }
    }
}
