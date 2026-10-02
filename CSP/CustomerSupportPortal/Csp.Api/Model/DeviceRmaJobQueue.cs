using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.Model.Enums;

namespace Csp.Api.Model
{
    public class DeviceRmaJobQueue
    {
        public long JobUniqueId { get; set; }

        public string SerialNumber { get; set; }

        public int StatusCode { get; set; }

        public JobStatus Status { get; set; }

        public string StatusMessage { get; set; }

        public JobVerdict Verdict { get; set; }
        
    }

}
