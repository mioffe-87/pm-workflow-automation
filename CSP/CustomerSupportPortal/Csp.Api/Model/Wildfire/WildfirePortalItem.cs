using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.Wildfire
{
    public class WildfirePortalItem
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("portalName")]
        public string PortalName { get; set; }

        [JsonProperty("keyUrl")]
        public string KeyUrl { get; set; }
    }
}
