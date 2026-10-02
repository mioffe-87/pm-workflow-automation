using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Csp.Api.Model
{
    public class ActivationLinksLookupItemDetails
    {
        [JsonProperty("product")]
        public string Product { get; set; }

        [JsonProperty("group")]
        public string Group { get; set; }

        [JsonProperty("subGroup")]
        public string SubGroup { get; set; }

        [JsonProperty("evalId")]
        public string EvalId { get; set; }

        [JsonProperty("registrationDate")]
        public string RegistrationDate { get; set; }

        [JsonProperty("partNumber")]
        public string PartNumber { get; set; }

        [JsonProperty("expirationDate")]
        public string ExpirationDate { get; set; }

        [JsonProperty("quantity")]
        public string Quantity { get; set; }

        [JsonProperty("useCount")]
        public string UseCount { get; set; }

        [JsonProperty("saasQuantity")]
        public string SaasQuantity { get; set; }

        [JsonProperty("bundleLicenseId")]
        public string BundleLicenseId { get; set; }
    }
}
