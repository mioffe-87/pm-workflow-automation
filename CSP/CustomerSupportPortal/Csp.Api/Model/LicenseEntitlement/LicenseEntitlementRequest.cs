using System;
using Csp.Api.Model.Attributes;
using System.Data;
using Csp.Api.DataAccess.HelperClasses.Literals;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Csp.Api.Model {
    public class LicenseEntitlementRequest
    {
        [JsonProperty("tenant_id")]
        public string TenantId { set; get; }        

        [JsonProperty("entitlement_type")]
        public string[] EntitlementType { set; get; }
	}
}
