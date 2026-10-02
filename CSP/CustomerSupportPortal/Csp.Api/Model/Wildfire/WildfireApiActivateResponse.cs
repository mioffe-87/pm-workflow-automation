using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.Wildfire
{
    public class WildfireApiActivateResponse : WildfireApiResponseBase
    {
        [JsonProperty("productType")]
        public WildFireApiActivationProductType ProductType { get; set; }

        [JsonProperty("serialNumber")]
        public string SerialNumber { get; set; }

        [JsonProperty("authCode")]
        public string AuthCode { get; set; }

        [JsonProperty("quantity")]
        public dynamic Quantity { get; set; }
    }
}
