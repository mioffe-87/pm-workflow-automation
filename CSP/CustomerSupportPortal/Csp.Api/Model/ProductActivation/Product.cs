using System.Collections.Generic;

namespace Csp.Api.Model
{
    public class Product
    {
        public string Name { get; set; }
        public string ProductType { get; set; }
        public List<Detail> Details { get; set; }
    }
}