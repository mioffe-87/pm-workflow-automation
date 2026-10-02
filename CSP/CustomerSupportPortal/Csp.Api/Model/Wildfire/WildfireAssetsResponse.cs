using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.Wildfire
{
    public class WildfireAssetsResponse<T>
        where T: class, new()
    {
        [JsonProperty("items")]
        public IEnumerable<T> Items { get; set; }

        [JsonProperty("total")]
        public int Total { get; set; }
    }
}
