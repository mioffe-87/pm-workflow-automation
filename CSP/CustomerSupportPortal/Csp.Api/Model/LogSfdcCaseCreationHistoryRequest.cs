using Newtonsoft.Json;

namespace Csp.Api.Model
{
    public class LogSfdcCaseCreationHistoryRequest
    {

        [JsonProperty(PropertyName = "caseNumber")]
        public string CaseNumber { get; set; }

        [JsonProperty(PropertyName = "userAccountId")]
        public long UserAccountId { get; set; }

        [JsonProperty(PropertyName = "supportAccountId")]
        public long SupportAccountId { get; set; }
    }
}
