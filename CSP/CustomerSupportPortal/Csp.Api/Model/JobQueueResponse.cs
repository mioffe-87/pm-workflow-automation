using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Csp.Api.Model
{
    public class JobQueueResponse : BaseJobResponse
    {
        [JsonProperty("jobs_queued", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<long, RmaDeviceDetails> JobsQueued { get; set; }
    }
}
