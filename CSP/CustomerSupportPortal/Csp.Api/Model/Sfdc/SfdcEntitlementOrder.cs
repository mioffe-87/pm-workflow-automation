using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Csp.Api.Model.Sfdc
{
    public class SfdcEntitlementOrder
    {
        [JsonProperty("orderId")]
        public string OrderId { get; set; }

        [JsonProperty("orderDate")]
        public string OrderDate { get; set; }
    }
}
