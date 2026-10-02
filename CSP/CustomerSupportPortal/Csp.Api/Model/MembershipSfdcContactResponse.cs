using Newtonsoft.Json;

namespace Csp.Api.Model
{
    public class MembershipSfdcContactValidationResponse
    {
        [JsonProperty("canCreateCase")]
        public bool CanCreateCase { get; set; }
        [JsonProperty("isContactExists")]
        public bool IsContactExists { get; set; }
        [JsonProperty("validationMessage")]
        public string ValidationMessage { get; set; }
    }
}
