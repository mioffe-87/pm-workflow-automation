using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Csp.Api.Model;
using Newtonsoft.Json;

namespace Csp.Web.DataTransferObjects
{
    public class AppNotificationRequest
    {
        [JsonProperty(PropertyName = "event")]
        public AppNotification NotificationEvent { get; set; }
        [JsonProperty(PropertyName = "key")]
        public string Key { get; set; }
    }
}