using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model
{
    public class BulkRegistrationModel
    {
        public long Id { get; set; }
        public long SupportAccountId { get; set; }
        public int BulkRegistrationStateId { get; set; }
        
        public long? CreatedById { get; set; }
        public string CreatedByEmail { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime? SubmittedOn { get; set; }

        public List<BulkRegistrationDeviceModel> BulkRegistrationDevices { get; set; }
    }
}
