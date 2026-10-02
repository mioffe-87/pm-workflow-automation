using System;
using Csp.Api.Model.Attributes;
using System.Linq;
using Csp.Api.DataAccess.HelperClasses.Literals;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Csp.Api.Model {
    public class PanServiceRegenerateMagicLinkRequest
    {
        [JsonProperty("authCode")]
        public string AuthCode { set; get; }       
    }   
}
