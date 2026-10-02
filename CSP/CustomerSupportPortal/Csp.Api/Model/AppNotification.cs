using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.Model;

namespace Csp.Api.Model
{
    public class AppNotification
    {
        [JsonProperty(PropertyName = "category")]
        public string Category { get; set; }

        [JsonProperty(PropertyName = "source")]
        public string Source { get; set; }

        [JsonProperty(PropertyName = "deliveryChannel")]
        public List<string> DeliveryChannel { get; set; }

        [JsonProperty(PropertyName = "appData")]
        public AppNotificationData AppData { get; set; }
        /*
            Sample JSON
         
            event: {
                category: "CASE_UPDATES",
	            source: "SALESFORCE",
	            deliveryChannel: ["WEB"],
	            appData: {
		            userId: "",
		            accountId: "",
		            entityId: ""
	            }
            }
         */
    }

    public class Events
    {
        [JsonProperty("event")]
        public AppNotification Event { get; set; }
    }

    public class Options 
    {
        public string ProjectId;
        public string TopicId;
    };
}
