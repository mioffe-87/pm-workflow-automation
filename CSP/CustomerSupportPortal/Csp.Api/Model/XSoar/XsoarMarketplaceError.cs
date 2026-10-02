using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.Xsoar
{
    public class XsoarMarketplaceError
    {
        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("locations")]
        public List<XsoarMarketplaceError> Locations { get; set; }

        [JsonProperty("path")]
        public List<string> Path { get; set; }
    }

}