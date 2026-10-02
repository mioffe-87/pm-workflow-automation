using System;
using Csp.Api.Model.Attributes;
using System.Data;
using Csp.Api.DataAccess.HelperClasses.Literals;
using Newtonsoft.Json;

namespace Csp.Api.Model {

    public class ActivationLinkLicenseRequest
    {
        [JsonProperty("SKU")]
        public string Sku { set; get; }

        [JsonProperty("grace")]
        public int Grace { set; get; }

        [JsonProperty("term")]
        public int Term { set; get; }

        [JsonProperty("saasquantity")]
        public int SaasQuantity { set; get; }

        [JsonProperty("usecount")]
        public int UseCount { set; get; }
    }
}
