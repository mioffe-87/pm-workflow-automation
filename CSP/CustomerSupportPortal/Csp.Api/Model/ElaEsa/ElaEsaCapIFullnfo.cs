using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.ElaEsa
{
    public class ElaEsaCapIFullnfo<T> : ElaEsaCapInfo
        where T: class, new()
    {
        [JsonProperty("devices")]
        public IEnumerable<T> Devices { get; set; }
    }
}
