using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace Csp.Web.DataTransferObjects
{
    public class AppNotificationResponse
    {
        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }
        [JsonProperty(PropertyName = "isSuccess")]
        public bool IsSuccess { get; set; }
    }
}