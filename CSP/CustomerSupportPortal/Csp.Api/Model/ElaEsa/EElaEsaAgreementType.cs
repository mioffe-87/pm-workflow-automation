using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.ElaEsa
{
    [Flags]
    public enum EElaEsaAgreementType
    {
        [JsonProperty("none")]
        None = 0,
        [JsonProperty("ela")]
        Ela = 1,
        [JsonProperty("esa")]
        Esa = 2,
        [JsonProperty("all")]
        All = 4
    }
}
