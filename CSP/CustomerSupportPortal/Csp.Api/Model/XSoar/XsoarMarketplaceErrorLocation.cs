using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.Xsoar
{
    public class XsoarMarketplaceErrorLocation
    {
        [JsonProperty("line")]
        public int Line { get; set; }

        [JsonProperty("column")]
        public int column { get; set; }
    }

}