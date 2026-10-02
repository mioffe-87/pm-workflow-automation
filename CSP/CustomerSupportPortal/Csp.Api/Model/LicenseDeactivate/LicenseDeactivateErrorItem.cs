using Newtonsoft.Json;

namespace Csp.Api.Model
{
    public class LicenseDeactivateErrorItem
    {
        [JsonProperty("error_code")]
        public int ErrorCode { set; get; }

        [JsonProperty("message")]
        public string Message { set; get; }       
    }   
}
