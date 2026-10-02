using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model
{
    public class BulkRegistrationHistoryListItem
    {
        public long BulkRegistrationId { get; set; }
        public DateTime? ActivationDate { get; set; }
        public string Username { get; set; }
        public int NoOfDevicesRegistered { get; set; }
        public int NoOfLicensesApplied { get; set; }
        public DateTime? CreatedOn { get; set; }
    }
}
