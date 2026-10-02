using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model
{
    public class LogSfdcCaseCreationHistoryResponse
    {
        [JsonProperty(PropertyName = "logCaseCreationHistoryId")]
        public long LogCaseCreationHistoryId { get; set; }

        [JsonProperty(PropertyName = "caseNumber")]
        public string CaseNumber { get; set; }

        [JsonProperty(PropertyName = "userAccountId")]
        public long UserAccountId { get; set; }

        [JsonProperty(PropertyName = "supportAccountId")]
        public long SupportAccountId { get; set; }

        [JsonProperty(PropertyName = "createdOn")]
        public DateTime CreatedOn { get; set; }

    }
}
