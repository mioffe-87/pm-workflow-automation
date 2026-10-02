using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model
{
    public class CsspOrderHistoryItems
    {
        public int CsspShoppingCartId { get; set; }
        public int CsspShoppingCartItemId { get; set; }
        public long SkuPartmasterId { get; set; }
        public string PartNumber { get; set; }
        public string SkuDescription { get; set; }
        public string ModelName { get; set; }
        public string AuthorizationCode { get; set; }
        public string ExpirationDate { get; set; }
        public long? UseCount { get; set; }
    }
}
