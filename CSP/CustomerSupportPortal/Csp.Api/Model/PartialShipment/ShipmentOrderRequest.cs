using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.PartialShipment
{
   public class ShipmentOrderRequest
    {
        [JsonProperty("referenceId")]
        public string ReferenceId { get; set; }
        [JsonProperty("masterOrderNumber")]
        public string MasterOrderNumber { get; set; }
        [JsonProperty("userAccountId")]
        public long UserAccountId { get; set; }
        [JsonProperty("supportAccountId")]
        public long SupportAccountId { get; set; }            

    }
}
