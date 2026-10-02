using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.PartialShipment
{
    public class MasterOrder
    {
        public long Id { get; set; }

        public string MasterOrderNumber { get; set; }

        public int? SoldToSapId { get; set; }

        public int? EndCustomerSapId { get; set; }

        public long SupportAccountId { get; set; }

        public long UserAccountId { get; set; }

        public string AdminEmail { get; set; }

        public long TenantId { get; set; }

        public string ReferenceId { get; set; }

        public DateTime? CreatedOn { get; set; }

        public long? CreatedBy { get; set; }

        public DateTime? UpdatedOn { get; set; }

        public long? UpdatedBy { get; set; }
    }
}
