using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.Wildfire
{
    public class WildfireApiActivationDetailsResponse : WildfireApiResponseBase
    {
        [JsonProperty("productType")]
        public WildFireApiActivationProductType ProductType { get; set; }

        [JsonProperty("authCode")]
        public string AuthCode { get; set; }

        [JsonProperty("serialNumber")]
        public string SerialNumber { get; set; }

        [JsonProperty("license")]
        public WildfireActivationLicense License { get; set; }

        [JsonProperty("cspIds")]
        public IEnumerable<dynamic> CspIds { get; set; }

        public static WildfireApiActivationDetailsResponse SetError(string messag)
        {
            return new WildfireApiActivationDetailsResponse
            {
                Success = false,
                Message = messag
            };
        }
    }
}
