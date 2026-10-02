using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model
{
    public class CsspOrderSapInput
    {
        public int CartId { get; set; }
        public string CartNumber { get; set; }
        public List<string> OrderItems { get; set; }
        public long SupportAccountId { get; set; }
        public string SpSfdcAccountId { get; set; }
        public string DistiSfdcAccountId { get; set; }
        public string AscSfdcAccountId { get; set; }
        public string SalesRepAccountId { get; set; }
        public int? SoldToSalesOrg { get; set; }
        public string OrderedByUser { get; set; }
    }
}
