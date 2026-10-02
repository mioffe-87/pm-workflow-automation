using System;
using Csp.Api.Model.Attributes;
using System.Linq;
using Csp.Api.DataAccess.HelperClasses.Literals;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Csp.Api.Model { 
    public class LicenseEntitlementsEntitlements
    {
        [JsonProperty("entitlement_type")]
        public string[] EntitlementType { set; get; }

        [JsonProperty("ngfw")]
        public LicenseEntitlementNgfw Ngfw { set; get; }        
    }
}
