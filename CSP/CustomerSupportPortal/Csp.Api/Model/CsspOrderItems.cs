using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model
{
    public class CsspOrderItems
    {
        public int CsspProductId { get; set; }
        public string PartNumber { get; set; }
        public string SkuDescription { get; set; }
    }
}
