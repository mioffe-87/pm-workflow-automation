using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model
{
    public class JobQueueRequest
    {
        public List<RmaDeviceDetails> RmaDeviceDetails { get; set; }
    }
}
