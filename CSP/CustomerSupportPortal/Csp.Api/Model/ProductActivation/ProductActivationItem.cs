using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model
{
    public class ProductActivationItem
    {
        public long? Id { get; set; }
        public Order Order { get; set; }
        public string Status { get; set; }
        public string ActivationDate { get; set; }
        public string OrderType { get; set; }
        public string ActivationLink { get; set; }
        public List<Product> Products { get; set; }
    }
}
