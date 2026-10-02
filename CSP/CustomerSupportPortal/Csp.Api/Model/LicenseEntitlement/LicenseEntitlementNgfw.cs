using System;
using Csp.Api.Model.Attributes;
using System.Linq;
using Csp.Api.DataAccess.HelperClasses.Literals;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Csp.Api.Model {
    public class LicenseEntitlementNgfw
    {
        [JsonProperty("status")]
        public string Status { set; get; }

        [JsonProperty("max_expiration")]
        public DateTime? MaxExpiration { set; get; }

        [JsonProperty("licenses")]
        public List<LicenseEntitlementLicenses> Licenses { set; get; }
    }
}
