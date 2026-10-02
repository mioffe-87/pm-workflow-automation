using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model
{
    public class BulkDeviceTransferModel
    {
        public long Id { get; set; }
        public long SourceSupportAccountId { get; set; }
        public long DestinationSupportAccountId { get; set; }
        public int BulkTransferStateId { get; set; }

        public long? TransferredById { get; set; }
        public DateTime? CreatedOn { get; set; }

        public List<string> BulkTransferDevices { get; set; }
        public List<string> BulkTransferVMs { get; set; }
    }
}
