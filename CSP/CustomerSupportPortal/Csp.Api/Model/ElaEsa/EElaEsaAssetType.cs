using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.ElaEsa
{
    public enum EElaEsaAssetType
    {
        [JsonProperty("none")]
        None = 0,
        [JsonProperty("hw")]
        Hw = 1,
        [JsonProperty("vm-series")]
        VmSeries = 2,
        [JsonProperty("traps")]
        Traps = 3,
        [JsonProperty("all")]
        All = 4
    }
}
