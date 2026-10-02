using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.PartialShipment
{
    public class MasterContract
    {
        public long MasterOrderId { get; set; }

        public string MasterOrderNumber { get; set; }

        public string ParentOrderNumber { get; set; }

        public string PanwQuote { get; set; }

        public string AdminEmail { get; set; }

        public string Status { get; set; }
    }

    public enum MasterContractStatus
    {
        Open,
        Fulfilled
    }

}
