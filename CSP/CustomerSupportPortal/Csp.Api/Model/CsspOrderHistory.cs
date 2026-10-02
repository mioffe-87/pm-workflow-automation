using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model
{
    public class CsspOrderHistory
    {
        public int CsspShoppingCartId { get; set; }
        public string CsspCartNumber { get; set; }
        public long SupportAccountId { get; set; }
        public string SapOrderId { get; set; }
        public string OrderStatus { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string OrderedBy
        {
            get
            {
                return (FirstName + " " + LastName).Trim();
            }
        }
        public DateTime OrderDate { get; set; }
    }
}
