using System;
using Newtonsoft.Json;

namespace Csp.Api.Model
{
    public class LoginHistoryModel
    {
        public long LoginHistoryId { get; set; }
        [JsonProperty("sessionId")]
        public string SessionId { get; set; }
        [JsonProperty("targetUserAccountId")]
        public long? UserAccountId { get; set; }
        [JsonProperty("supportAccountId")]
        public long? SupportAccountId { get; set; }
        [JsonProperty("loginTime")]
        public string LoginTimeAsString { get; set; }
        public DateTime? LoginTime { get; set; }
        [JsonProperty("logoutTime")]
        public string LogoutTimeAsString { get; set; }
        [JsonProperty("loginStatus")]
        public string LoginStatus { get; set; }
        [JsonProperty("isOffline")]
        public bool IsOffline { get; set; }
        [JsonProperty("browser")]
        public string Browser { get; set; }
        [JsonProperty("platform")]
        public string Platform { get; set; }
        [JsonProperty("sourceIpAddress")]
        public string SourceIpAddress { get; set; }
        [JsonProperty("impersonatorEmail")]
        public string ImpersonateUser { get; set; }
        [JsonProperty("requestId")]
        public string ImpersonateRequestId { get; set; }
    }
}
