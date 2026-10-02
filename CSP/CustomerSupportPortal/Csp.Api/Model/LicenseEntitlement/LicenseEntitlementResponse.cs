using System;
using Csp.Api.Model.Attributes;
using System.Linq;
using Csp.Api.DataAccess.HelperClasses.Literals;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Csp.Api.Model {
    public class LicenseEntitlementResponse
    {
        [JsonProperty("status")]
        public string Status { set; get; }

        [JsonProperty("message")]
        public string Message { set; get; }

        [JsonProperty("error_code")]
        public int ErrorCode { set; get; }

        [JsonProperty("tenant")]
        public LicenseEntitlementTenant Tenant { set; get; }

        [JsonProperty("entitlements")]
        public LicenseEntitlementsEntitlements Entitlements { set; get; }

        public bool IsValid()
        {
            return Status == "SUCCESS"
                && Entitlements != null;
        }
    }   
}
