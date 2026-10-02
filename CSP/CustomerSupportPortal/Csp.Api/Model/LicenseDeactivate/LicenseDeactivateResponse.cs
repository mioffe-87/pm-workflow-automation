using Newtonsoft.Json;

namespace Csp.Api.Model
{
    public class LicenseDeactivateResponse
    {
        [JsonProperty("sku")]
        public string Sku { set; get; }

        [JsonProperty("auth_code")]
        public string AuthCode { set; get; }

        [JsonProperty("success")]
        public bool Success { set; get; }

        [JsonProperty("error_details")]
        public LicenseDeactivateErrorItem ErrorDetails { set; get; }
    }   
}
