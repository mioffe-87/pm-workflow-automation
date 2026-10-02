using System;
using Csp.Api.Model.Attributes;
using System.Data;
using Csp.Api.DataAccess.HelperClasses.Literals;
using Newtonsoft.Json;

namespace Csp.Api.Model {

    public class ActivationLinkLicenseResponse
    {
        [JsonProperty("SKU")]
        public string Sku { set; get; }

        [JsonProperty("serialNumber")]
        public string SerialNumber { set; get; }

        [JsonProperty("productSubGroup")]
        public string ProductSubGroup { set; get; }

        [JsonProperty("productGroup")]
        public string ProductGroup { set; get; }

        [JsonProperty("expirationDate")]
        public string ExpirationDate { set; get; }

        [JsonProperty("displayName")]
        public string DisplayName { set; get; }

        [JsonProperty("authorizationCode")]
        public string AuthorizationCode { set; get; }

        [JsonProperty("activationLink")]
        public string ActivationLink { set; get; }
    }
}
