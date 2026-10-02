using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model
{
    public class CsspOrderDetails
    {
        public int CsspShoppingCartId { get; set; }
        public string CsspShoppingCartNumber { get; set; }        
        public string OrderedBy { get; set; }
        public string SapOrderId { get; set; }
        public string Status { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string CsspSku { get; set; }
        public string CsspSkuDesc { get; set; }
        public string AuthorizationCode { get; set; }
    }
}
