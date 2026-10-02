using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.Wildfire
{
    public class WildfireAssetIteemQuantity
    {
        [JsonProperty("submissions")]
        public string Submissions { get; set; }

        [JsonProperty("queries")]
        public string Queries { get; set; }

        public override string ToString()
        {
            return "Submissions: " + Submissions + ", Queries: " + Queries;
        }
    }
}
