using Newtonsoft.Json;

namespace Csp.Api.Model
{
    public class DeleteTrialLicenseRequest
    {
        [JsonProperty(PropertyName = "user_email")]
        public string UserEmail { get; set; }
        [JsonProperty(PropertyName = "serial_number")]
        public string SerialNumber { get; set; }

        [JsonProperty(PropertyName = "user_account_id")]
        public string UserAccountId { get; set; }

        [JsonProperty(Required = Required.AllowNull, PropertyName = "reason")]
        public string Reason { get; set; }
    }
}