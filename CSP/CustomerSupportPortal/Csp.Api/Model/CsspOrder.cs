using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model
{
    public class CsspOrder
    {
        public int CsspOrderId { get; set; }
        public string CsspCartNumber { get; set; }
        public string SapOrderNumber { get; set; }
        public long SupportAccountId { get; set; }
        public IEnumerable<CsspOrderItems> OrderItems { get; set; }
    }
}
