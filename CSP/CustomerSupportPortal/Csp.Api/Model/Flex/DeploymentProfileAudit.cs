using Newtonsoft.Json;
using System.Collections.Generic;

namespace Csp.Api.Model
{
    public class DeploymentProfileAudit
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("dateTime")]
        public string CreatedOn { get; set; }

        [JsonProperty("profileName")]
        public string ProfileName { get; set; }

        [JsonProperty("authCode")]
        public string AuthCode { get; set; }

        [JsonProperty("modifiedBy")]
        public string ModifiedBy { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

    }

 
    public class DeploymentProfileAuditDetail
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("changeDetail")]
        public string ChangeDetail { get; set; }

        [JsonProperty("previousVersion")]
        public List<string> PreviousVersion { get; set; }

        [JsonProperty("newVersion")]
        public List<string> NewVersion { get; set; }
    }

}
