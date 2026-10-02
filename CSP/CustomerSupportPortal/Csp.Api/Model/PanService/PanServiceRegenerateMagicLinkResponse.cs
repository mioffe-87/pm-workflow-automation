using System;
using Csp.Api.Model.Attributes;
using System.Linq;
using Csp.Api.DataAccess.HelperClasses.Literals;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Csp.Api.Model {
    public class PanServiceRegenerateMagicLinkResponse
    {
        [JsonProperty("isSuccess")]
        public bool Success { set; get; }

        [JsonProperty("licenses")]
        public List<PanServiceRegenerateMagicLinkLicense> Licenses { set; get; }

        [JsonProperty("message")]
        public string Message { set; get; }       
    }   
}
