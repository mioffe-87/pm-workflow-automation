using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model
{
 [Serializable]
    public class ScheduledJobs
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
    }

    
}
