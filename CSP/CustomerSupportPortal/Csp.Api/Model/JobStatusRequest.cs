using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Csp.Api.Model
{
    public class JobStatusRequest
    {
        [JsonProperty("job_ids")]
        public List<long> JobIds { get; set; }
    }
}
