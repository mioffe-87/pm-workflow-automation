using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Csp.Api.Model
{
    public class AppNotificationData
    {
        [JsonProperty(PropertyName = "userId")]
        public string UserId { get; set; }
        [JsonProperty(PropertyName = "accountId")]
        public string AccountId { get; set; }
        [JsonProperty(PropertyName = "entityId")]
        public string EntityId { get; set; }

        [JsonProperty(PropertyName = "targetaccountId")]
        public string TargetAccountId { get; set; }
        [JsonProperty(PropertyName = "status")]
        public int Status { get; set; }
        [JsonProperty(PropertyName = "additionalData")]
        public AdditionalDetails AdditionalData { get; set; }
        /*
            Sample JSON
         
	        appData: {
		        userId: "",
		        accountId: "",
		        entityId: ""
	        }
         */
    }

    public class AdditionalDetails
    {
        [JsonProperty(PropertyName = "newAccountSetting")]
        public int NewAccountSetting { get; set; }
        [JsonProperty(PropertyName = "currentAccountSetting")]
        public int CurrentAccountSetting { get; set; }
        [JsonProperty(PropertyName = "status")]
        public string MembershipStatus { get; set; }
        [JsonProperty(PropertyName = "isExpired")]
        public bool IsExpired { get; set; }
    }
}
