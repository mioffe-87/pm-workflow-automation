using System;
using Csp.Api.Model.Attributes;
using System.Linq;
using Csp.Api.DataAccess.HelperClasses.Literals;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Csp.Api.Model {
    public class PanServiceRegenerateMagicLinkLicense
    {
        [JsonProperty("activationLink")]
        public string ActivationLink { set; get; }

        [JsonProperty("assetType")]
        public string AssetType { set; get; }

        [JsonProperty("authorizationCode")]
        public string AuthorizationCode { set; get; }

        [JsonProperty("capacity")]
        public string Capacity { set; get; }

        [JsonProperty("displayName")]
        public string DisplayName { set; get; }

        [JsonProperty("expirationDate")]
        public string ExpirationDate { set; get; }

        [JsonProperty("licenseType")]
        public string LicenseType { set; get; }

        [JsonProperty("productGroup")]
        public string ProductGroup { set; get; }

        [JsonProperty("productSubGroup")]
        public string ProductSubGroup { set; get; }

        [JsonProperty("serialNumber")]
        public string SerialNumber { set; get; }

        [JsonProperty("sku")]
        public string Sku { set; get; }
    }
}
