using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model
{
    public class TrainingPartner
    {
        [JsonProperty(PropertyName = "support_account_id")]
        public long SupportAccountId { get; set; }

        [JsonProperty(PropertyName = "user_account_id")]
        public long UserAccountId { get; set; }

        [JsonProperty(PropertyName = "choices")]
        public List<Choice> Choices { get; set; }
    }

    public class Choice
    {
        [JsonProperty(PropertyName = "preference_id")]
        public string PerferenceId { get; set; }

        [JsonProperty(PropertyName = "preferred_order")]
        public long PreferredOrder { get; set; }

        [JsonProperty(PropertyName = "atp_id")]
        public string AtpId { get; set; }

        [JsonProperty(PropertyName = "atp_name")]
        public string AtpName { get; set; }

        [JsonProperty(PropertyName = "atp_contact_email")]
        public string AtpContactEmail { get; set; }
    }
}
