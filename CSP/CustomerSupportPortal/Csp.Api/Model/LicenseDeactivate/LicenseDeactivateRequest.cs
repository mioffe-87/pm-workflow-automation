using Newtonsoft.Json;

namespace Csp.Api.Model
{
    public class LicenseDeactivateRequest
    {
        [JsonProperty("auth_code")]
        public string AuthCode { set; get; }        

        [JsonProperty("serial_number")]
        public string SerialNumber { set; get; }
	}
}
