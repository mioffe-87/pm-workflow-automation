using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.Ion
{
    public class IonResponse
    {
        [JsonProperty("tenants")]
        public IEnumerable<string> Tenants { get; set; }

        [JsonProperty("currentTenant")]
        public string CurrentTenant { get; set; }

        [JsonProperty("items")]
        public IEnumerable<IonItem> Items { get; set; }

        [JsonProperty("total")]
        public int Total { get; set; }
    }
}
