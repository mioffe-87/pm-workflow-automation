using System;
using Csp.Api.Model.Attributes;
using System.Data;
using Csp.Api.DataAccess.HelperClasses.Literals;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Csp.Api.Model {

    public class ActivationLinkRequest
    {
        [JsonProperty("SFAccountId")]
        public string SfAccountId { set; get; }

        [JsonProperty("SupportAccountId")]
        public long SupportAccountId { set; get; }

        [JsonProperty("UserEmail")]
        public string UserEmail { set; get; }

        [JsonProperty("SerialNumber")]
        public string SerialNumber { set; get; }

        [JsonProperty("OrderType")]
        public string OrderType { set; get; }

        [JsonProperty("Licenses")]
        public IEnumerable<ActivationLinkLicenseRequest> Licenses { set; get; }
	}
}
