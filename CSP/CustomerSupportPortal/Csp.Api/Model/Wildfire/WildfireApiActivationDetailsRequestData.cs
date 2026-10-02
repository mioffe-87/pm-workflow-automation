using System;
using Newtonsoft.Json;

namespace Csp.Api.Model.Wildfire
{
    public class WildfireApiActivationDetailsRequestData
    {
        [JsonProperty("product_type")]
        public WildFireApiActivationProductType ProductType { get; set; }

        [JsonProperty("auth_code")]
        public string AuthCode { get; set; }

        public void Validate()
        {
            if ((int)ProductType <= 0
                || ProductType > WildFireApiActivationProductType.FedrampSupportAWF)
            {
                throw new ApplicationException("WF API GetActivationDetails Unknown Product Type.");
            }

            if (ProductType == WildFireApiActivationProductType.FedrampSupportAWF
                && string.IsNullOrWhiteSpace(AuthCode))
            {
                throw new ApplicationException("WF API GetActivationDetails Details Payload AuthCode is required.");
            }
        }
    }
}
