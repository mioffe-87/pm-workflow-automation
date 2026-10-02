using System;
using Csp.Api.Model.Attributes;
using System.Linq;
using Csp.Api.DataAccess.HelperClasses.Literals;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Csp.Api.Model {

    public class ActivationLinkResponse 
    {
        [JsonProperty("isSuccess")]
        public bool IsSuccess { set; get; }

        [JsonProperty("message")]
        public string Message { set; get; }

        [JsonProperty("licenses")]
        public IEnumerable<ActivationLinkLicenseResponse> Licenses { set; get; }

        public bool IsValid()
        {
            return IsSuccess
                && Licenses != null
                && Licenses.Any();
        }
    }
}
