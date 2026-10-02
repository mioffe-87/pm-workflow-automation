using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Csp.Api.Model
{
    public class AlertModel
    {
        //public string messageHeader { get; set; }
        //public string messageDetails { get; set; }
        //public string errorMessage { get; set; }

        [JsonProperty("header")]
        public string Header { get; set; }

        [JsonProperty("detail")]
        public string Detail { get; set; }

        [JsonIgnore]
        public Severity Severity { get; set; }

        [JsonProperty("type")]
        public string Type
        {
            get { return Severity.ToString().ToLower(); }
        }
        [JsonProperty("isPortalTac")]
        public bool isPortalTac { get; set; }
    }

    public class AlertModelList
    {
        public List<AlertModel> AlertMessages { get; set; }
    }
    public class AlertMessagesModel
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("messageHeader")]
        public string Header { get; set; }

        [JsonProperty("messageDetails")]
        public string Detail { get; set; }
      
        [JsonProperty("active")]
        public bool active { get; set; }

        [JsonProperty("createdOn")]
        public string CreatedOn { get; set; }

        [JsonProperty("createdByUserAccountId")]
        public long CreatedByUserAccountId { get; set; }

        [JsonProperty("scope")]
        public int Scope { get; set; }

        [JsonProperty("severity")]
        public int Severity { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("isPortalTac")]
        public bool isPortalTac { get; set; }
        

    }

    public class AlertMessagesResponse
    {
        [JsonProperty("data")]
        public List<AlertMessagesModel> data { get; set; }
    }
    public enum Severity
    {
        Critical = 1,
        Warning = 2,
        Info = 3
    }

    public enum Scope
    {
        None = 0,
        Authenticated = 1,
        UnAuthenticated = 2,
        Both = 3
    }

    public class AlertMessageRequest
    {
        [JsonProperty("scopes")]
        public List<int> Scopes { get; set; }

    }
}
